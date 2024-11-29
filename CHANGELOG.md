# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.2.0] - 2024-10-29

### Changed

- Migrate from self-hosted UI to [StardewUI](https://www.nexusmods.com/stardewvalley/mods/28870) (Framework).
- Sort already-gifted and non-giftable NPCs at the end of the grid.
- Show full item tooltips for the selected gift item and per-NPC queued items.
- Increase scale of portrait background, so that borders are thicker.
- Switch to semi-dynamic menu layout to make menu usable on very small (720p) and very large (2K/4K) displays.

### Fixed

- Tint for reaction (taste) emojis is now correct for NPCs with queued gifts.

## [0.1.0] - 2024-09-03

### Added

- Initial release.
- Outgoing gift menu (UI).
- Gift distributor and rules checker.
- Console commands `dryrun` and `receiveall`.
- Generic Mod Config Menu page.

[Unreleased]: https://github.com/focustense/StardewPenPals/compare/v0.2.0...HEAD
[0.2.0]: https://github.com/focustense/StardewPenPals/compare/v0.1.0...v0.2.0
[0.1.0]: https://github.com/focustense/StardewPenPals/tree/v0.1.0
