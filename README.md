# Sensate IoT - Smart Energy

The Sensate IoT Smart Energy project implements an IoT solution for (Dutch)
Smart Meters. The project consists of several repository's:

- DSMR parser (this repo);
- DSMR web client (implementator of this service)
- Several customer facing apps

## DSMR Web Client

The DMSR Web Client Service connects to Sensate IoT as a live data client. This way
DSMR data is routed to this service by the data platform. This service is responsible
for serveral things:

- parsing the telegrams
- writing energy measurements back to Sensate IoT

In order to receive data, several sensors need to be configured with the correct routing
configuration. To support all functionality (electrical data, gas data and environmental
data), 4 sensors are required:

- Raw telegram sensor;
- Gas sensor;
- Electrical/power sensor;
- Environment sensor.

The raw telegram sensor needs to following triggers:

- Raw telegram sensor:
  - Live data trigger to itself (for DSMR telegrams);
  - Live data trigger to the environmental sensor.

## Parsing

While this service is responsible for the fact that telegrams are parsed, it doesn't
actually parse them itself. This service implements a WCF client to the DSMR Parser
Service.
