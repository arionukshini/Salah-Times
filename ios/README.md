# iOS test build

This is a small SwiftUI port of the Windows app's daily prayer times, countdown, extra times, and saved city selector. It includes an App Intent to open the app from Shortcuts. It targets iOS 17 or later.

The bundled timetable is the existing Kosovo table reused by the desktop app. It is a fixed 2016 schedule and has no February 29 row; the app uses February 28 on leap day. Times and countdowns use Kosovo local time.

## Build an IPA without a Mac

The repository includes a manual GitHub Actions workflow at `.github/workflows/ios-test-ipa.yml`. After this folder and workflow are pushed to GitHub, open **Actions → iOS test IPA → Run workflow**. Download the `SalahTimes-unsigned-ipa` artifact and extract `SalahTimes-unsigned.ipa` from it.

The IPA is unsigned. It cannot be installed directly by tapping it. A sideloading tool must sign it for your device using your Apple ID, or an Apple development team must sign an archive. The GitHub Actions workflow only builds and packages the app; it does not collect signing credentials.

For Simulator testing on a Mac, open `SalahTimes.xcodeproj`, choose the `SalahTimes` scheme and an iPhone simulator, then run it. The Simulator build is separate from the device IPA.
