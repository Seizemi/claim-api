Review the generated code with three passes:

Pass 1 - Code Quality:
- Are naming conventions consistent across all files?
- Are all handlers following the same pattern?
- Are there any unused imports or dead code?
- Is the code DRY without being over-abstracted?

Pass 2 - Performance:
- Are there any N+1 query issues in EF Core calls?
- Are there any unnecessary database round-trips?
- Are all async methods properly awaited?
- Are CancellationTokens passed through the entire call chain?

Pass 3 - Security:
- Are all user inputs validated before processing?
- Are there any SQL injection or XSS vulnerabilities?
- Are sensitive data fields excluded from responses?
- Are sensitive data fields excluded from logging?

For each issue found, show the file, the problem, and the fix.
