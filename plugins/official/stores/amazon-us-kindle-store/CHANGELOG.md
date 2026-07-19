# Changelog

## 1.0.4 - 2026-07-17

- Raised the default per-search result ceiling to the PluginContracts safety maximum of 100.
- Raised the bounded Amazon remote-page default to 25 pages (maximum 50).
- Log one logical “search starting” event per operation; page fetches use separate progress logs with an operation id.

## 1.0.2 - 2026-07-16

- Improved Amazon search HTTP requests with conservative desktop-browser headers.
- Added transient retries for HTTP 429/502/503/504 with exponential backoff and Retry-After support.
- Replaced raw URL error text with friendly store messages; keep technical details in diagnostic logs only.
- Detect Amazon robot-check/CAPTCHA pages and surface a clear non-bypass message.
- Fetch multiple Amazon result pages until the configured maximum is reached.
- Updated selectors for current Amazon title/author faceout markup.

## 1.0.1 - 2026-07-16

- Fixed `ObjectDisposedException` during store search by using a plugin-owned long-lived `HttpClient` instead of the host init-scoped HTTP client.
- Plugin package metadata now declares `requiresRestart = true`.

## 1.0.0

- Initial Amazon US Kindle Store official first-party BookAtrium plugin.
- Search-only integration for Amazon.com Kindle listings.
- Opens selected product pages in the user's default browser through BookAtrium host activation.
- No Amazon API, account access, purchasing, downloads, DRM handling, telemetry, or analytics.
