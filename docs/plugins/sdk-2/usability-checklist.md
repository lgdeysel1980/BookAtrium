# Plugin API 2.0 usability checklist

Acceptance checklist for task-first authoring.

## Scaffold → pack

- [ ] Project references **only** `BookAtrium.PluginContracts` **2.0.0**
- [ ] Author edits **one** public plugin class (plus optional settings POCO)
- [ ] `bookatrium-plugin validate` succeeds (when CLI is available)
- [ ] `bookatrium-plugin pack` emits `.bookplugin` + `.sha256` without hand-written `plugin.json`

## Domain bases

- [ ] `StorePlugin.SearchAsync` returns results the Stores UI can show
- [ ] `MetadataSourcePlugin.SearchAsync` returns metadata the host can apply
- [ ] Reader / writer / conversion / device plugins implement only their domain methods

## Context & settings

- [ ] Networked plugins declare `NetworkHosts`; host HTTP rejects others
- [ ] `[PluginSetting]` + `SettingsType` appear in Settings UI after install
- [ ] `PluginTestContext` can run a fixture HTTP search without launching BookAtrium

## Publishing path (third-party / community plugins)

- [ ] `bookatrium-plugin prepare-release` produces registry-entry scaffold
- [ ] Reusable workflow `lgdeysel1980/BookAtrium/.github/workflows/plugin-build.yml@main` builds/tests/packs a plugin repo
- [ ] Registry PR targets [BookAtrium-Community-Plugins](https://github.com/lgdeysel1980/BookAtrium-Community-Plugins)
- [ ] Do **not** submit official BookAtrium plugins to the community catalogue; official releases use [`plugins/official/`](../../../plugins/official/) and [`registries/official-plugins.json`](../../../registries/official-plugins.json)
