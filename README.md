# cf-sync-folders

This tool automatically mirrors particular folders to another folder in the background for backup purposes. It can be used for 
fixed & removable drives. You could back up machine folders & removable drive folders to a NAS and also back up the NAS later.

The tool currently only works with a local or network file system. The code for Dropbox/OneDrive/Google Drive needs to be completed
so that it could be used with cloud storage.

Example of Usage
----------------
1) Each machine with the tool installed mirrors particular folders to the NAS. E.g. Documents, Pictures etc.
2) When removable drive #1 is plugged in to a machine with the tool installed then particular folders are mirrored to the NAS.
3) When removable drive #2 is plugged in to a machine with the tool installed then the NAS folders from above are backed up.

Modes of Execution
------------------
1) Interactively with a UI. This is typically for administration functions such as configuring the folders to mirror.
2) From the system tray (Usually launched on startup).
3) From the command line.

Removable Drives
----------------
Removable drives will have a uniquely named verification text file created in the root so that the tool can identify the particular
device even if the drive letter is different each time that the device is plugged in. E.g. MyWorkSSD.verify. We could use a drive 
label instead but someone else might plug in a removable drive with the same label and the tool might unintentionally modify it.

Sync Configuration
------------------
A sync configuration defines a named group of folders that should be mirrored. Typically it relates to a single device such as a
machine or removable drive. The UI displays each sync configuration in the dropdown list.

Configurations that are only applicable for a specific machine (E.g. Backing up the local drive to a NAS) should have the Machine
property set. Other configurations (E.g. For a removable SSD that can be connected to any computer) should leave the property
empty.

The source or destination folder for the configuration also supports placeholders that will be replaced at runtime. E.g. Machine
name, current date etc. The full list is visible from the View Placeholders button when modifying the configuration.
