## [2.1.1] - 2026-03-25

### Changed
- IAP reinitialization is now done in the async loop instead of callbacks. And now is limited to retries

## [2.1.0] - 2026-03-24

### Added
- `MinMax` and `MinMaxInt` utility structs with Random and Clamp helpers.
- `DebugIAP` implementation for simulating in-app purchases in editor/testing.
- 
### Fixed
- IAP crashes on MAC and iOS


## [2.0.0] - 2025-10-06

### Changed
- Updated Unity IAP module to support new 5.0.1 version

### Fixed
- Tag drawer lags


## [1.0.1] - 2025-03-10

### Changed
- IViewData value get

### Fixed
- Scriptable services default names
- Service installer not running correctly
- Player prefs encrypted key errors
- Service initializer Unity services using into directive


## [1.0.0] - 2025-02-24

### Added
- Created first version of package