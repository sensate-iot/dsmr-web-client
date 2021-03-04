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

## Parsing

While this service is responsible for the fact that telegrams are parsed, it doesn't
actually parse them itself. This service implements a WCF client to the DSMR Parser
Service.

