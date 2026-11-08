import SwiftUI

struct ContentView: View {
    @AppStorage("selectedCity") private var city = "Prishtina"
    @State private var selectedTab = 0

    private let background = Color(red: 3 / 255, green: 10 / 255, blue: 18 / 255)
    private let panel = Color(red: 23 / 255, green: 36 / 255, blue: 54 / 255)
    private let blue = Color(red: 15 / 255, green: 135 / 255, blue: 241 / 255)

    var body: some View {
        TimelineView(.periodic(from: .now, by: 1)) { context in
            let today = PrayerData.schedule(for: context.date, city: city)
            ScrollView {
                VStack(spacing: 20) {
                    VStack(spacing: 8) {
                        Image(systemName: "moon.stars.fill")
                            .font(.system(size: 34))
                            .foregroundStyle(blue)
                        Text("Salah Times")
                            .font(.largeTitle.bold())
                            .foregroundStyle(.white)
                        if let next = PrayerData.nextPrayer(after: context.date, city: city) {
                            Text("\(next.0) edhe: \(countdown(to: next.1, from: context.date))")
                                .font(.subheadline.monospacedDigit())
                                .foregroundStyle(.cyan)
                        }
                    }
                    .frame(maxWidth: .infinity)

                    Picker("Times", selection: $selectedTab) {
                        Text("Namazi").tag(0)
                        Text("Extra").tag(1)
                    }
                    .pickerStyle(.segmented)

                    if let today {
                        VStack(spacing: 10) {
                            if selectedTab == 0 {
                                ForEach(today.prayers.indices, id: \.self) { index in
                                    timeRow(today.prayers[index].0, value: PrayerSchedule.clock(today.prayers[index].1))
                                }
                            } else {
                                ForEach(today.extras.indices, id: \.self) { index in
                                    timeRow(today.extras[index].0, value: today.extras[index].1)
                                }
                            }
                        }
                    } else {
                        Text("Nuk u gjet orari për këtë datë.")
                            .foregroundStyle(.white)
                            .padding()
                    }

                    HStack {
                        Text("Qyteti")
                            .foregroundStyle(.white.opacity(0.7))
                        Spacer()
                        Picker("Qyteti", selection: $city) {
                            ForEach(PrayerData.cities, id: \.name) { item in
                                Text(item.name).tag(item.name)
                            }
                        }
                        .tint(blue)
                    }
                    .padding(.horizontal, 8)
                }
                .padding(20)
                .frame(maxWidth: 560)
                .frame(maxWidth: .infinity)
            }
        }
        .background(background.ignoresSafeArea())
        .preferredColorScheme(.dark)
    }

    private func timeRow(_ name: String, value: String) -> some View {
        HStack {
            Text(name)
                .foregroundStyle(.white.opacity(0.75))
            Spacer(minLength: 8)
            Text(value)
                .font(.headline.monospacedDigit())
                .foregroundStyle(blue)
        }
        .padding(18)
        .frame(maxWidth: .infinity)
        .background(panel, in: RoundedRectangle(cornerRadius: 14))
    }

    private func countdown(to target: Date, from now: Date) -> String {
        let seconds = max(0, Int(target.timeIntervalSince(now)))
        return String(format: "%02d:%02d:%02d", seconds / 3600, seconds / 60 % 60, seconds % 60)
    }
}
