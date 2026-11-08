import AppIntents
import SwiftUI

@main
struct SalahTimesApp: App {
    var body: some Scene {
        WindowGroup {
            ContentView()
        }
    }
}

struct OpenSalahTimesIntent: AppIntent {
    static var title: LocalizedStringResource = "Open Salah Times"
    static var description = IntentDescription("Show today's prayer times.")
    static var openAppWhenRun = true

    func perform() async throws -> some IntentResult {
        .result()
    }
}

struct SalahTimesShortcuts: AppShortcutsProvider {
    static var appShortcuts: [AppShortcut] {
        AppShortcut(
            intent: OpenSalahTimesIntent(),
            phrases: ["Open \(.applicationName)"],
            shortTitle: "Prayer times",
            systemImageName: "clock"
        )
    }
}
