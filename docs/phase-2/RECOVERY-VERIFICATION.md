# Phase 2 recovery evidence

Verified on 26 November 2026 (Africa/Lagos).

## Backend GitHub verification

Repository: [SEN_PROJECT_GROUP_14](https://github.com/divine3054/SEN_PROJECT_GROUP_14).

Backend commit: [bfb18e46acfd88eae3dbd34b649cd214ef31eff9](https://github.com/divine3054/SEN_PROJECT_GROUP_14/commit/bfb18e46acfd88eae3dbd34b649cd214ef31eff9).

Commit subject: `Phase 2: Add core models, JSON storage, and complaint service`.

`git push -u origin main` succeeded. The GitHub REST commit endpoint returned that SHA for main, and the recursive tree endpoint returned these backend paths and Git blob hashes:

- `ComplaintManagementSystem/Models/Enums.cs`: `e7cc7fff5c767466e146e8a5a12bd5d1b3466a63`.
- `ComplaintManagementSystem/Models/User.cs`: `4fd137d5586db53f1c1b3478fa9a7ecbd1a2e1b0`.
- `ComplaintManagementSystem/Models/Complaint.cs`: `e867c6c8e69a9cb0f22f92a8c91b3285e2ad1632`.
- `ComplaintManagementSystem/Data/StorageService.cs`: `20412dec204b17f0d2882f03568ce0fddab8f8be`.
- `ComplaintManagementSystem/Services/ComplaintService.cs`: `2501b0b5a3dd1e08e9d445d917e14d33cf6b2d54`.

The project page was launched in the user's default browser. Remote file verification used GitHub's API; a browser screenshot was not captured.

## Build and backend verification

Windows .NET SDK 10.0.404 was downloaded from Microsoft's official dotnet installer into ignored `.tools/dotnet`. Existing Visual Studio 2022 tools lacked the required SDK. A sandboxed build was blocked from reading Windows SDK locations; the successful build ran with approved access to those locations.

Command: `.tools/dotnet/dotnet.exe build ComplaintManagementSystem.sln --configuration Release --verbosity minimal --nologo`.

Result: **Build succeeded; 0 warnings; 0 errors**. See [full build output](BUILD-VERIFICATION.txt). This validates the whole solution, including the unchanged placeholder forms, using the .NET CLI. Visual Studio GUI build output was not observed.

An isolated console harness under ignored `.tools/phase2-smoke` compiled the five backend source files and exercised seven backend checks using a unique temporary data directory. No real complaint/user JSON files were created in the app directory. See [smoke output](BACKEND-SMOKE-VERIFICATION.txt).

## Diagram verification

[design.drawio](design.drawio) is valid XML with eight pages. All exports were inspected visually. The required four architecture/UML files are:

- [3-Tier Architecture](exports/01-architecture.png): 2400 x 1800 pixels.
- [Use Case Diagram](exports/02-use-cases.png): 3000 x 2480 pixels.
- [Class Diagram](exports/03-classes.png): 3200 x 2360 pixels.
- [Submit and Track Sequence Diagram](exports/04-sequence.png): 3000 x 2660 pixels.

Four further exports cover LoginForm, CustomerForm's Submit and Track tabs, and AgentForm. Control names follow the requested Phase 2 contract.

## Human approval

Member 3 has not supplied an approval confirmation in this session. The [project log](PROJECT_LOG.md) contains the sign-off record with its approval fields explicitly pending. No connected master-report document session is available. Technical verification cannot substitute for Member 3's signature or confirmation that their master report is updated.
