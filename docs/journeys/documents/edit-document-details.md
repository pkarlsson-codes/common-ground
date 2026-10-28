# Edit Document Details

1. User navigates to Documents and clicks a document row; a panel opens titled with the document's title and current file name
2. User clicks "Download" to download the current version in a new tab
3. Details: the panel shows the sensitivity badge and expiry, plus a form with Title, Sensitivity (Public, Internal, Confidential, Restricted), and Expires On
4. User edits any of them and clicks "Save Details" (enabled only when something changed); clearing Expires On removes the expiry
5. On save, toast "Document updated" is shown and the library row reflects the change (an expiry within 90 days also shows the document in [View expiring documents](view-expiring-documents.md))
6. Tags: user types into "New tag" and clicks "Add Tag"; tags are trimmed and lowercased, and duplicates are ignored. User clicks "×" on a tag to remove it
7. Versions: every version is listed newest first with file name, size, and upload date; "Download" on a version downloads that version
8. Attached To: lists the records the document is attached to; for a home, "Open Home" opens that home's panel on the Homes page
9. If a save fails, the server's error message is shown as a toast
