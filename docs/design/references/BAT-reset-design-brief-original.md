# Basic’s Advanced Timer
## Design brief for the reset

BAT should be the place an agency PR consultant keeps track of a day as it changes. Keep it visible while they work, make it easy to capture a thought or switch accounts mid-task, and give them one place to put the day back in order. By Friday, they should be able to see where their hours went, spot accounts at risk of overservicing and enter their time in the agency’s timesheet with the right code and a clear description of the work.

### What the first version taught us

| What worked | What that means for the reset |
| --- | --- |
| The always-on-top pill prompted you to track time, pick up work and notice what was coming next. | Keep a compact desktop presence. Make the active account and recording state immediately clear, with pause or resume, break and account switching available from the pill. |
| The horizontal timeline gave you one place to record, plan and adjust a day. Adding and moving time already felt broadly intuitive. | Keep the horizontal timeline as the main workspace. Use the existing gestures as the baseline and fix the cases where placement, selection or undo feels unpredictable. |
| Notes on time blocks gave you a record of what the hours were spent on. | Make block descriptions and work categories quick to add, easy to check and visible when entering timesheets. |
| A quick look at the week and individual account budgets helped you catch overservicing. | Give weekly hours and budget pace a clear place in the dashboard. Show the period each budget covers so a monthly budget reads differently from a weekly total. |
| The bat and the compact display gave the app character. Segmented text worked for short, scrolling readouts. | Keep the bat, even if the name changes. Use display effects where they help a short readout. Give timeline labels, weekly figures and editing controls clear type and sufficient contrast in Light and Dark. |

### How the day should work

**Capture a note before dealing with its details.** A desktop shortcut should open a small, text-first composer from the app you’re already using, including on another monitor. Save the note with text alone. Keep it in recent captures so you can come back and add an account or reminder later. From the timeline, offer a direct way to add a note at a chosen time with that time already filled in. Both routes should feel like the same composer. A quick “check the press release” should be as easy to save as a longer note is to reopen and develop. Give call notes and release ideas a roomier editor with lightweight Markdown for headings, bullets and links. Use the same quick capture flow for either kind of note. Make notes findable by account and text search, including notes saved without a reminder.

**Give time blocks, notes and meetings their own jobs.** The timeline brings them together. A time block records work and contributes to totals. A scheduled note brings a thought or action back to your attention. A meeting reserves time on the plan and may need a start, end and repeat settings. Make selection, opening details and dismissing an edit behave consistently. Give each item the controls its job needs. A meeting contributes to recorded time only when the user logs work. Moving a reminder changes when it comes back to you.

**Record what the time went on.** Make it quick to give a block a short description and choose a reusable work category, such as media outreach, drafting or a client call. Show the description on the timeline when there’s room and use a clear indicator on shorter blocks. Make it easy to spot blocks missing a description when reviewing the day or preparing a timesheet. Keep the description with the time block so it travels with the entry into the weekly handoff.

**Make a changed day easy to put right.** Start with the add, move, resize, select, merge, delete and undo behaviours that already work for time blocks. Test direct placement and editing for meetings and reminders as well. While moving anything, show where it will land and what snapping will do. Make the affected item and the result of deletion clear, especially for repeating meetings. The first prototype should show how someone reverses a mistaken edit across the day while their timer keeps running.

**Let the dashboard make use of space.** Use the current window as the first candidate for the minimum size and test it on a crowded day. When the user makes it taller, try giving the timeline more room for overlapping blocks and showing more of the selected record or notes. When it gets wider, compare a layout with those panels beside the timeline. Keep the selected item, timeline position and any note in progress as the window changes size. That gives us a way to judge any taller or split layout against the same work.

**Give the current task more room.** Bring time blocks and their controls forward when someone starts editing a block. When they open a cluster of nearby reminders, try a temporary zoom around the cluster so each one is easy to select and move. Let them return to the previous view with their place and drafts intact. Let empty lanes collapse and let people tuck away the notes panel or meeting lane if they rarely use it. Give that space back to the timeline and keep a clear way to bring either feature back.

**Make the type of account clear and map it to the agency’s timesheet.** Make retainers, projects and new business easy to tell apart in the account picker and weekly review. Let people track each prospect or pitch under its own name in BAT and assign several of those accounts to the same external New Business code. Show the combined hours under that code in the weekly handoff, with an expandable list of the original blocks by day. Each block keeps its prospect, time and description, so the same view supports entry as a total or block by block.

**Make end-of-week entry a short, deliberate task.** Keep the weekly totals and budget check easy to reach. From there, try a vertical, chronological list of blocks grouped by day, with start and end, account, duration, work category and description. Make descriptions readable and easy to copy from the handoff view. Flag missing descriptions for review where the destination requires one. Compare the list with the existing Timesheet table when entering a sample week into Float or a similar tracker. See which view makes it easier to pick up where you left off and enter each block accurately. Reconcile the grouped and individual totals against the same recorded time.

### What the first prototype needs to prove

Use one busy agency day for the first prototype, with the rest of the week filled in for the handoff. Start in another app while tracking Account A. Capture “check the press release”, then find it again, tag it to Account B and give it a reminder. Add a second note directly on the timeline and reschedule it alongside two nearby reminders. A client call moves, so shift the meeting, adjust the work blocks and describe what the time went on. Undo a mistaken edit, take a break and switch accounts from the pill. Reopen the call notes, add a heading and bullets, then resize the dashboard mid-edit. Try the same day with the meeting lane and Notes panel tucked away. Finish by checking an account budget and entering time for two new-business prospects under the same external code, with a description missing from one block.

Run the prototype through that sequence and compare with the equivalent tasks in Alpha.

- **Capture takes less effort.** Compare the steps from another app to a saved note. Find it after deferring the account and reminder, then add structure to a longer note without re-entering it.
- **The day stays understandable as it changes.** Someone new to BAT can explain what’s planned, what’s recorded and what’s running. They can select the right reminder in a cluster, correct an edit and resize without losing their place. With notes and meetings tucked away, the time blocks get more room.
- **The week is easy to reconcile.** The user can identify the budget period, spot and fill in the missing block description, and enter time for both prospects under the right code. The combined hours match the individual blocks, and each description is available in the handoff.

Prototype the pill, capture and adaptive timeline together first. Include block descriptions and a small Markdown editing flow, then test the weekly handoff with the same blocks. Keep advanced recurrence, a full rich-text editor, new integrations, decorative dashboard themes and naming decisions outside that first pass. A prototype can use sample data. Before asking anyone to switch for daily use, decide how their existing time records, accounts and notes will come across and check that their hours still reconcile.

### Decisions to make from the prototype

- **How prominent should elapsed time be?** Compare the current clock-led pill with one led by account and recording state, with elapsed time still available. Use both during a real working day and see which helps you notice forgotten or misallocated tracking.
- **Where should notes live before they’re scheduled?** Test how easily you find an untagged capture the next day, and whether you can turn the same note into a reminder or develop it into a longer briefing.
- **When should the layout adapt?** Compare the day at rest, during a block edit and with a cluster of reminders open. Try each at the current size and with more height or width. Decide the minimum size from how easily people can read blocks and edit notes, and check where they land when closing a reminder cluster.
- **Which work categories save time?** Try a small set with real examples of drafting, calls and outreach. See whether they save typing and help people prepare descriptions for the agency’s timesheet.

### Reference for the build

The Alpha guide describes the existing pill controls, horizontal timeline, shared note and meeting form, weekly account budgets and CSV export. Alpha already supports notes on time blocks and gives the full timeline height to blocks on days without meetings. The display notes also document a Timesheet view with account-by-day values. Those are the starting points for comparison. The desktop project already references shared Core and Storage packages, so a fresh interface can be explored with those packages as the starting point for the clock, ledger and export behaviour. These references describe the repository. The Windows interaction checks above still need to happen in the app.

[Alpha user guide](https://github.com/BasicallyGod/Basic-Advanced-Timer/blob/alpha/app/README.md) · [Alpha display and Timesheet notes](https://github.com/BasicallyGod/Basic-Advanced-Timer/blob/alpha/docs/design/display-data-presentation.md) · [Alpha desktop project](https://github.com/BasicallyGod/Basic-Advanced-Timer/blob/alpha/app/src/Timer.Desktop/Timer.Desktop.csproj)
