# Group 14 application demonstration

[Watch/download the MP4](Group14-Demonstration.mp4).

This is an automated screen recording of the actual WinForms application screens, using isolated synthetic complaint data. It demonstrates invalid/valid login, required-field checks, submission with a returned reference, tracking and blank/unknown input handling, Support Agent filtering/updating, and the saved resolution after reopening the services.

The recorder hosts the existing production forms in one fixed demonstration window so only that application window is captured. Message requests use auto-closing demonstration dialogs to allow unattended recording. The normal application continues to use its standard message boxes. The recording has visible step captions and no narration.

This artifact is a demonstration, not a signature, independent stakeholder test or claim that a human performed the walkthrough. Named human acceptance remains in docs/submission/ACCEPTANCE.md. Screenshots and recorded step timestamps are retained alongside this file. No live complaint/user data was used.

Reproduction requires Windows, .NET 10 SDK and an FFmpeg build supporting gdigrab and H.264. Build tools/DemoRecorder, then pass the absolute encoder path, output directory and a new empty data directory. The recorder refuses an occupied data directory.
