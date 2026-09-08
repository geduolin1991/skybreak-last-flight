# Security policy

The current supported release is SKYBREAK 1.8. The browser edition runs locally in the player's browser; it has no player account, payment service, multiplayer server, or online score authority. Saves belong to the local browser or macOS app.

Please [report a vulnerability privately](https://github.com/geduolin1991/skybreak-last-flight/security/advisories/new). Include the version, affected file or URL, impact, and minimal reproduction steps. Do not post credentials or personal information in public issues. Do not load-test GitHub's infrastructure.

## Build and publishing controls

- Keep secrets out of source, generated assets, logs, and release archives. GitHub secret scanning and push protection are enabled; CI also scans Git history with a pinned, checksum-verified Gitleaks binary.
- CI has read-only repository permissions, no saved Git credential, and no access to deployment secrets. Dependency alerts are enabled. The security workflow runs on pushes and pull requests, not on a timer.
- Source and Pages branches reject deletion and force pushes. Published version tags reject updates and deletion. Normal authorized commits can still change the site, so account security remains essential.
- Web builds finish with `Tools/version_web_assets.py`, which applies `Tools/harden_web.py` after Unity expands template variables. The result restricts scripts and connections, hashes the inline bootstrap and checks external JavaScript/CSS integrity. `Tools/check_web_security.py` verifies this output.
- Unity currently requires `blob:` scripts/workers and WebAssembly compilation; CSS uses inline styles for touch layout. These necessary allowances are explicit. HTML metadata cannot enforce `frame-ancestors`, `X-Frame-Options`, `X-Content-Type-Options` or `Permissions-Policy`; GitHub Pages does not expose project-controlled response headers for this deployment.
- Editor MCP functionality is not a remotely reachable game service. The editor bridge uses loopback; runtime helper assemblies are separate from the editor's execution tools. Do not enable LAN binding or expose developer ports through a tunnel without separate authentication and review.
- macOS downloads use an ad-hoc signature, not Apple Developer ID signing or notarization. Checksums verify the archive against the published record, not the identity of a publisher whose account has been compromised.

An audit records the inspected revision and evidence. A clean scanner result does not prove absence of unknown vulnerabilities. Browsers, Unity, and development tools still need security updates.
