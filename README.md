TimeTrack is an application to help manage my timesheets. 

The tools provided in our latest system made it difficult to know where my time was going, so I made this to assist.

Features:  

Time Entry
- Accept a time start, end, case/reference number, and a description. 
- Time entries will default to working hours (7AM - 7PM) with AM being the preference, if ambiguous
- Display the list of time entries for the current calendar date
- The delete key can remove the currently selected entry
- The following are acceptable examples of time entries:
7:00 AM, 700, 730, 705, 7, 7:00 PM, 700 PM, 7.00, 7;00, 1900

Saving & Loading
- All entries are written to file as a csv as they are entered/modified
- Entries will be loaded from file, if there is a file with a matching date specified.

Statistics
- Display total time for the day, and the amount of gaps in hours/minutes if any records without a case number are present.
- The "Lunch" checkbox can be used to create a 1 hour time record which does not count towards the total time, or gaps.

Exporting
- The export button will copy the currently selected time record to the clipboard, in the format of:

"x:xx AM - x:xx PM  
Discription text here"

![Screenshot](/Resources/Screenshot.png?raw=true)
