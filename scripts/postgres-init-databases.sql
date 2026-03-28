-- One PostgreSQL instance; bounded contexts use separate databases (matches local appsettings).
-- Run as a superuser against your local PostgreSQL instance, then apply EF migrations per API.
CREATE DATABASE administration_configuration_dev;
CREATE DATABASE audit_provenance_dev;
CREATE DATABASE clinical_analytics_dev;
CREATE DATABASE clinical_interoperability_dev;
CREATE DATABASE device_registry_dev;
CREATE DATABASE financial_interoperability_dev;
CREATE DATABASE measurement_acquisition_dev;
CREATE DATABASE measurement_validation_dev;
CREATE DATABASE query_read_model_dev;
CREATE DATABASE realtime_surveillance_dev;
CREATE DATABASE reporting_dev;
CREATE DATABASE replay_recovery_dev;
CREATE DATABASE signal_conditioning_dev;
CREATE DATABASE terminology_conformance_dev;
CREATE DATABASE treatment_session_dev;
CREATE DATABASE workflow_orchestrator_dev;
