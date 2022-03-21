# AMS.Profile.2
## Description
Same thing as AMS.Profile, but with the ability to change entry and section names inside an .ini file.

## Added Features
Here are the features that don't exist on the original that I added:
  - Ini.ChangeSectionName(string, string)
    - You give the original name and the new name, and the old is replaced by the new.
  - Ini.ChangeEntryName(string, string, string)
    - You give the specific section, the original entry name, and the new entry name, and the old is replaced by the new.

## How It Works
### Ini.ChangeSection(string, string)
1. Uses the [StreamReader](https://docs.microsoft.com/en-us/dotnet/api/system.io.streamreader?view=net-6.0) class to get the original file contents.
2. Replaces the section name found in the file string with the new name.
3. Uses the [File](https://docs.microsoft.com/en-us/dotnet/api/system.io.file?view=net-6.0) class to delete the original file and create a new blank one.
4. Uses the [StreamWriter](https://docs.microsoft.com/en-us/dotnet/api/system.io.streamwriter?view=net-6.0) class to re-write the file with the new section name.

### Ini.ChangeEntryName(string, string, string)
This is an iteration loop by the way.
1. Gets the value of the entry in the current iteration.
2. Uses **Ini.RemoveEntry(string, string)** in the original dll to remove the entry in the current iteration.
3. Re-creates the entry with its original value in the current iteration, and uses the new name specified if it matches the original name given.

## Code Has Been Tested
I have tested both methods with an ini file containing several sections, and several entries for each section.

## Download
[AMS.Profile.2.dll](https://github.com/Lexz-08/AMS.Profile.2/releases/download/ams.profile.2/AMS.Profile.2.dll)

## Original DLL
[AMS.Profile](https://www.nuget.org/packages/Ams.Profile/)
