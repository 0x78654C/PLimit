# PLimit 1.0.2

Released August 12, 2026

PLimit 1.0.2 focuses on making saved process settings faster, safer, and more reliable while improving error handling across process-management operations.

## Highlights

- Saved settings now contain only values that were explicitly changed, producing smaller and cleaner `processes.json` files.
- Existing settings remain compatible: legacy `True`/`False` values are supported, and obsolete `null` fields are removed automatically on the next save.
- Settings are loaded in a single pass instead of repeatedly reading the JSON file, and saved efficiency-mode settings are now restored correctly.
- CPU priority, affinity, I/O priority, efficiency mode, process priority boost, and thread priority boost are saved only after Windows applies the requested change successfully.
- Process priority boost now requests only the Windows access rights it needs, fixing failures with processes that reject broader handle access.
- Safer process selection and PID handling prevents stale or missing processes from causing errors.
- Missing, unreadable, or malformed settings files no longer crash startup.
- Improved cleanup of process and thread handles, the single-instance mutex, and About-dialog image resources.
- Added regression tests for settings storage, legacy setting values, JSON handling, and setting-state parsing.

## Upgrade notes

No manual migration is required. Existing settings files will continue to work and will be normalized the next time they are saved.

**Full changelog:** [v1.0.1...1.0.2 changes](https://github.com/0x78654C/PLimit/compare/v1.0.1...07806f0d2534a64a1b9ce94feda76b12f94fc494)
