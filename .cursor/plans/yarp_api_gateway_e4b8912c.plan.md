---
name: YARP API Gateway
overview: Add RealtimePlatform.ApiGateway (YARP) as the single HTTPS entry for browsers, with route ordering for overlapping paths; point pdms-web VITE_API_BASE_URL at the gateway and allow CORS for the Vite dev origin.
todos:
  - id: gateway-proj
    content: Create RealtimePlatform.ApiGateway project + appsettings routes/clusters + slnx
    status: completed
  - id: react-cors
    content: Update pdms-web env example + Vite CSP connect-src for gateway port
    status: completed
isProject: true
---

# YARP gateway

## Behavior

- **Reverse proxy / load balancer**: YARP clusters target each microservice Kestrel URL; `LoadBalancingPolicy` `RoundRobin` when multiple destinations are listed (scale-out).
- **Auth**: Same C5 JWT pipeline as APIs; forward `Authorization`, `X-Tenant-Id`, and correlation id. In Development with `DevelopmentBypass`, proxy does not require an authenticated principal (matches downstream bypass).
- **Health**: Gateway `/health` is anonymous liveness; microservice health remains on each service.

## Mermaid

```mermaid
flowchart LR
  React[pdms-web] -->|HTTPS / JWT| Gateway[ApiGateway YARP]
  Gateway --> Svc1[Microservices 5001-5016]
```
