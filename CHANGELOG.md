# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
### Added
- Unsubscribe functionality
- Resubscription timer

### Updated
- Application configuration

## [1.2.2] - 26-03-2021
### Added
- Sensate IoT Gateway HTTP status code logging
- Response logging

### Updated
- Data platform response logging
- Version support policy
- Integration documentation

## [1.2.1] - 12-03-2021
### Added
- Logging of fatal exceptions
- Reconnect/resubscribe logic when a websocket connection breaks:
  - Implement a re-authenticate flow
  - Implement a re-authorize flow

### Updated
- Ping error handling
- Connection disposing

## [1.2.0] - 12-03-2021
### Added
- Additional package references
- Code style analysis

### Updated
- Improved code based on code analysis feedback
- Improved stop handling

## [1.1.0] - 10-03-2021
### Added
- Environimental sensor support
- Listener abstraction

### Updated
- Configuration settings
- Sensate IoT websocket listener implementation

## [1.0.0] - 04-03-2021
### Added
- A power sensor target for electrical measurements
- Console application host

### Updated
- Parser Service client
- Sensor configuration
- Gas flow logging
- The amount of logging statements

## [0.1.0] - 02-03-2021
### Added
- Initial import
- Sensate IoT Data Platform integration
- Windows service application host
