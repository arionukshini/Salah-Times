import Foundation

struct PrayerSchedule {
    let fajr: Date
    let sunrise: Date
    let dhuhr: Date
    let asr: Date
    let maghrib: Date
    let isha: Date

    var prayers: [(String, Date)] {
        [("Sabahu", fajr), ("Dreka", dhuhr), ("Ikindia", asr),
         ("Akshami", maghrib), ("Jacia", isha)]
    }

    var extras: [(String, String)] {
        let tomorrowFajr = Self.calendar.date(byAdding: .day, value: 1, to: fajr)!
        let night = tomorrowFajr.timeIntervalSince(maghrib)
        return [
            ("Lindja e diellit", Self.clock(sunrise)),
            ("Duha", "\(Self.clock(sunrise.addingTimeInterval(15 * 60))) – \(Self.clock(dhuhr.addingTimeInterval(-10 * 60)))"),
            ("Mesi i natës", Self.clock(maghrib.addingTimeInterval(night / 2))),
            ("1/3 e natës", Self.clock(maghrib.addingTimeInterval(night / 3))),
            ("Namazi i natës", Self.clock(maghrib.addingTimeInterval(night * 2 / 3)))
        ]
    }

    static let zone = TimeZone(identifier: "Europe/Belgrade")!

    static var calendar: Calendar {
        var calendar = Calendar(identifier: .gregorian)
        calendar.timeZone = zone
        return calendar
    }

    static func clock(_ date: Date) -> String {
        let formatter = DateFormatter()
        formatter.timeZone = zone
        formatter.dateFormat = "HH:mm"
        return formatter.string(from: date)
    }
}

enum PrayerData {
    // The desktop app uses the same 2016 timetable for each year.
    private static let rows: [String: [String]] = {
        guard let url = Bundle.main.url(forResource: "PrayerTimes", withExtension: "json"),
              let data = try? Data(contentsOf: url),
              let rows = try? JSONDecoder().decode([String: [String]].self, from: data) else {
            return [:]
        }
        return rows
    }()

    static let cities: [(name: String, offset: Int)] = [
        ("Prishtina", -1), ("Ferizaj", -1), ("Gjilani", -1),
        ("Presheva", -2), ("Podujeva", -1), ("Sharri", 2), ("Vushtrria", -1)
    ]

    static func schedule(for date: Date, city: String) -> PrayerSchedule? {
        let calendar = PrayerSchedule.calendar
        let components = calendar.dateComponents([.day, .month], from: date)
        guard let day = components.day, let month = components.month else {
            return nil
        }
        // The source timetable has no leap-day row; use February 28 for that day.
        let key = month == 2 && day == 29 ? "28-02" : String(format: "%02d-%02d", day, month)
        guard let values = rows[key], values.count == 6 else {
            return nil
        }
        let offset = cities.first(where: { $0.name == city })?.offset ?? -1
        let midnight = calendar.startOfDay(for: date)
        func time(_ value: String) -> Date? {
            let parts = value.split(separator: ":").compactMap { Int($0) }
            guard parts.count == 2 else { return nil }
            return calendar.date(byAdding: .minute, value: parts[0] * 60 + parts[1] + offset, to: midnight)
        }
        guard let imsak = time(values[0]), let sunrise = time(values[1]),
              let dhuhr = time(values[2]), let asr = time(values[3]),
              let maghrib = time(values[4]), let isha = time(values[5]) else {
            return nil
        }
        return PrayerSchedule(fajr: imsak.addingTimeInterval(18 * 60),
                              sunrise: sunrise, dhuhr: dhuhr, asr: asr,
                              maghrib: maghrib, isha: isha)
    }

    static func nextPrayer(after now: Date, city: String) -> (String, Date)? {
        for day in [now, PrayerSchedule.calendar.date(byAdding: .day, value: 1, to: now)].compactMap({ $0 }) {
            if let prayer = schedule(for: day, city: city)?.prayers.first(where: { $0.1 > now }) {
                return prayer
            }
        }
        return nil
    }
}
