# Reqestra

**Self-hosted media requests, automation, and lifecycle management — independently maintained and based on [Ombi](https://github.com/Ombi-app/Ombi).**

[![Build](https://github.com/ExtremeFiretop/Reqestra/actions/workflows/build.yml/badge.svg?branch=MediaCleanupFeature)](https://github.com/ExtremeFiretop/Reqestra/actions/workflows/build.yml)
[![License: GPL v2](https://img.shields.io/badge/License-GPL_v2-blue.svg)](LICENSE)
[![Based on Ombi](https://img.shields.io/badge/based%20on-Ombi-orange.svg)](https://github.com/Ombi-app/Ombi)
[![Upstream](https://img.shields.io/badge/upstream-Ombi--app%2FOmbi-informational.svg)](https://github.com/Ombi-app/Ombi)

> [!IMPORTANT]
> **Reqestra is a separate downstream project, not an official Ombi release.** It is independently maintained and has diverged substantially from the upstream codebase. Bugs caused by Reqestra-specific behavior should be reported here rather than to the upstream Ombi maintainers.

## What is Reqestra?

Reqestra began as an Ombi fork focused on safer media cleanup and lifecycle automation. It has since grown into a broader self-hosted media request and automation project with extensive changes across request processing, Sonarr integration, failure recovery, authentication, dependency security, metadata handling, and the web UI.

Reqestra deliberately keeps **Ombi at its core**. The request-management model, major media-server integrations, and a large amount of the underlying application are inherited from Ombi. Reqestra builds on that foundation while developing its own identity, release path, and downstream feature set.

## Highlights of this fork

- **Media Cleanup lifecycle automation** — adds cleanup state and workflows for requested media, including safer handling of partial TV requests and destructive cleanup operations.
- **Stronger destructive-operation safeguards** — freezes deletion scope, persists cleanup state before external destructive calls, snapshots destination/settings identity, and revalidates request ownership and scope immediately before deletion.
- **More resilient Sonarr TV matching** — repairs missing provider identity, handles consolidated Sonarr series, tolerates safe title variants, and uses episode fingerprints when season numbering or metadata does not line up cleanly.
- **Improved failed-request recovery** — distinguishes retryable and deterministic failures, prevents endless retries for known-bad mappings, supports manual reprocessing, and repairs stale request-queue state after successful retries.
- **Safer Sonarr request handling** — rolls back partially-created Sonarr series when configuration fails and avoids unnecessary full-series searches in profile-override paths.
- **Plex resilience improvements** — preserves unknown history state when Plex history lookups fail rather than incorrectly treating failures as confirmed “never played” results.
- **Plex-only authentication mode** — administrators can optionally disable Ombi username/password authentication and require Plex sign-in while keeping the existing behavior as the default.
- **Custom branding improvements** — the configured custom logo is also used in the authenticated sidebar instead of being limited to the login experience.
- **API and metadata hardening** — rejects invalid/non-positive TMDB IDs, avoids empty TMDB `/find` requests, handles missing TV metadata more defensively, and validates Media Cleanup API enum values.
- **Security and dependency maintenance** — includes authentication and Media Cleanup mutation rate limiting plus targeted upgrades for MailKit, SharpCompress, Angular, Lodash, SignalR's `ws` dependency, and AutoMapper.
- **Discover/cache correctness fixes** — incorporates protections against TV Discover duplicate episode data, season cache-key collisions, and mutation of cached TMDB objects.

This list is intentionally a summary, not a complete changelog. See the repository history and releases for the detailed evolution of the fork.

## Relationship to upstream Ombi

Reqestra is derived from the open-source **Ombi** project created and maintained by **Jamie Rees (`tidusjar`)** and the wider Ombi contributor community.

- Upstream source: [Ombi-app/Ombi](https://github.com/Ombi-app/Ombi)
- Upstream website: [ombi.io](https://ombi.io/)
- Upstream documentation: [docs.ombi.app](https://docs.ombi.app/)
- License: [GNU GPL v2](LICENSE)

A significant portion of this repository remains Ombi code, and the original contributors are retained and credited below. Upstream fixes are reviewed and selectively reconciled where they remain applicable to this increasingly divergent codebase.

Nothing in this repository should be interpreted as an official Ombi release or as being endorsed or supported by the upstream maintainers.

## Installation and upgrades

Use **builds/releases from this repository** when you want Reqestra features. Official Ombi binaries do not contain Reqestra's downstream changes.

- Reqestra releases: [ExtremeFiretop/Reqestra releases](https://github.com/ExtremeFiretop/Reqestra/releases)
- Reqestra source: [ExtremeFiretop/Reqestra](https://github.com/ExtremeFiretop/Reqestra)
- Upstream installation documentation: [docs.ombi.app/installation](https://docs.ombi.app/installation/)
- Upstream reverse-proxy examples: [docs.ombi.app/info/reverse-proxy](https://docs.ombi.app/info/reverse-proxy/)

The upstream documentation is still useful for functionality inherited from Ombi, but this fork can differ in behavior, settings, dependencies, and release cadence.

## Inherited Ombi capabilities

Alongside the Reqestra-specific work above, the project retains the core capabilities that made Ombi its foundation, including:

- Movie, TV, episode, season, and music requests.
- Request approval and management workflows.
- Integration with services such as Sonarr, Radarr, Lidarr, Plex, Emby, and Jellyfin.
- User management with media-server and local authentication options.
- Availability/status synchronization with configured media servers.
- User notifications and request automation.
- Responsive web UI and configurable application branding.

## Issues and contributions

For problems that occur specifically in Reqestra, use this repository's [issue tracker](https://github.com/ExtremeFiretop/Reqestra/issues). If a problem is reproducible in an unmodified upstream Ombi build, it may also be appropriate to report it to the upstream project using their contribution guidelines.

# Contributors

<!-- readme: collaborators,contributors -start -->
<table>
<tr>
    <td align="center">
        <a href="https://github.com/tidusjar">
            <img src="https://avatars.githubusercontent.com/u/6642220?v=4" width="50;" alt="tidusjar"/>
            <br />
            <sub><b>Jamie</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/twanariens">
            <img src="https://avatars.githubusercontent.com/u/34845004?v=4" width="50;" alt="twanariens"/>
            <br />
            <sub><b>Twan Ariens</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/sephrat">
            <img src="https://avatars.githubusercontent.com/u/34862846?v=4" width="50;" alt="sephrat"/>
            <br />
            <sub><b>Sephrat</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/anojht">
            <img src="https://avatars.githubusercontent.com/u/21053678?v=4" width="50;" alt="anojht"/>
            <br />
            <sub><b>Anojh Thayaparan</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/Magikarplvl4">
            <img src="https://avatars.githubusercontent.com/u/2944704?v=4" width="50;" alt="Magikarplvl4"/>
            <br />
            <sub><b>Magikarp Lvl 4</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/MrTopCat">
            <img src="https://avatars.githubusercontent.com/u/774415?v=4" width="50;" alt="MrTopCat"/>
            <br />
            <sub><b>James Carty</b></sub>
        </a>
    </td></tr>
<tr>
    <td align="center">
        <a href="https://github.com/smcpeck">
            <img src="https://avatars.githubusercontent.com/u/8724583?v=4" width="50;" alt="smcpeck"/>
            <br />
            <sub><b>Shaun McPeck</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/AmyJeanes">
            <img src="https://avatars.githubusercontent.com/u/2363642?v=4" width="50;" alt="AmyJeanes"/>
            <br />
            <sub><b>Amy Jeanes</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/bernarden">
            <img src="https://avatars.githubusercontent.com/u/12289537?v=4" width="50;" alt="bernarden"/>
            <br />
            <sub><b>Victor Usoltsev</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/mike10010100">
            <img src="https://avatars.githubusercontent.com/u/3506604?v=4" width="50;" alt="mike10010100"/>
            <br />
            <sub><b>Michael Paulauski</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/dhruvb14">
            <img src="https://avatars.githubusercontent.com/u/4459649?v=4" width="50;" alt="dhruvb14"/>
            <br />
            <sub><b>Dhruv Bhavsar</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/joshuaboniface">
            <img src="https://avatars.githubusercontent.com/u/4031396?v=4" width="50;" alt="joshuaboniface"/>
            <br />
            <sub><b>Joshua M. Boniface</b></sub>
        </a>
    </td></tr>
<tr>
    <td align="center">
        <a href="https://github.com/bruvv">
            <img src="https://avatars.githubusercontent.com/u/3063928?v=4" width="50;" alt="bruvv"/>
            <br />
            <sub><b>Bruvv</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/Berserkir-Wolf">
            <img src="https://avatars.githubusercontent.com/u/15743201?v=4" width="50;" alt="Berserkir-Wolf"/>
            <br />
            <sub><b>Dyson Parkes</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/louis-lau">
            <img src="https://avatars.githubusercontent.com/u/1346804?v=4" width="50;" alt="louis-lau"/>
            <br />
            <sub><b>Louis Laureys</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/goldenpipes">
            <img src="https://avatars.githubusercontent.com/u/6140137?v=4" width="50;" alt="goldenpipes"/>
            <br />
            <sub><b>Goldenpipes</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/Namaneo">
            <img src="https://avatars.githubusercontent.com/u/6706489?v=4" width="50;" alt="Namaneo"/>
            <br />
            <sub><b>Julien Loir</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/ProtoJazz">
            <img src="https://avatars.githubusercontent.com/u/1490293?v=4" width="50;" alt="ProtoJazz"/>
            <br />
            <sub><b>Jim MacKenize</b></sub>
        </a>
    </td></tr>
<tr>
    <td align="center">
        <a href="https://github.com/GodKratos">
            <img src="https://avatars.githubusercontent.com/u/692616?v=4" width="50;" alt="GodKratos"/>
            <br />
            <sub><b>GodKratos</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/Unimatrix0">
            <img src="https://avatars.githubusercontent.com/u/357984?v=4" width="50;" alt="Unimatrix0"/>
            <br />
            <sub><b>Avi</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/kitzin">
            <img src="https://avatars.githubusercontent.com/u/3277321?v=4" width="50;" alt="kitzin"/>
            <br />
            <sub><b>Emil Kitti</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/grimsan55">
            <img src="https://avatars.githubusercontent.com/u/8499989?v=4" width="50;" alt="grimsan55"/>
            <br />
            <sub><b>Stefan</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/snyk-bot">
            <img src="https://avatars.githubusercontent.com/u/19733683?v=4" width="50;" alt="snyk-bot"/>
            <br />
            <sub><b>Snyk Bot</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/gtbuchanan">
            <img src="https://avatars.githubusercontent.com/u/715687?v=4" width="50;" alt="gtbuchanan"/>
            <br />
            <sub><b>Taylor Buchanan</b></sub>
        </a>
    </td></tr>
<tr>
    <td align="center">
        <a href="https://github.com/shiitake">
            <img src="https://avatars.githubusercontent.com/u/161589?v=4" width="50;" alt="shiitake"/>
            <br />
            <sub><b>Shannon Barrett</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/EstebanSmits">
            <img src="https://avatars.githubusercontent.com/u/1588767?v=4" width="50;" alt="EstebanSmits"/>
            <br />
            <sub><b>EstebanSmits</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/fservida">
            <img src="https://avatars.githubusercontent.com/u/501958?v=4" width="50;" alt="fservida"/>
            <br />
            <sub><b>Francesco Servida</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/Patricol">
            <img src="https://avatars.githubusercontent.com/u/13428020?v=4" width="50;" alt="Patricol"/>
            <br />
            <sub><b>Patrick Collins</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/xweskingx">
            <img src="https://avatars.githubusercontent.com/u/6268446?v=4" width="50;" alt="xweskingx"/>
            <br />
            <sub><b>Wesley King</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/jrh3k5">
            <img src="https://avatars.githubusercontent.com/u/545587?v=4" width="50;" alt="jrh3k5"/>
            <br />
            <sub><b>Joshua Hyde</b></sub>
        </a>
    </td></tr>
<tr>
    <td align="center">
        <a href="https://github.com/Fire-Swan">
            <img src="https://avatars.githubusercontent.com/u/60622768?v=4" width="50;" alt="Fire-Swan"/>
            <br />
            <sub><b>Fire-Swan</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/ombi-bot">
            <img src="https://avatars.githubusercontent.com/u/51722903?v=4" width="50;" alt="ombi-bot"/>
            <br />
            <sub><b>Ombi-bot</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/mhann">
            <img src="https://avatars.githubusercontent.com/u/17162399?v=4" width="50;" alt="mhann"/>
            <br />
            <sub><b>Mhann</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/dr3am37">
            <img src="https://avatars.githubusercontent.com/u/91037083?v=4" width="50;" alt="dr3am37"/>
            <br />
            <sub><b>Dr3amer</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/aptalca">
            <img src="https://avatars.githubusercontent.com/u/541623?v=4" width="50;" alt="aptalca"/>
            <br />
            <sub><b>Aptalca</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/stpanzer">
            <img src="https://avatars.githubusercontent.com/u/4676271?v=4" width="50;" alt="stpanzer"/>
            <br />
            <sub><b>Stephen Panzer</b></sub>
        </a>
    </td></tr>
<tr>
    <td align="center">
        <a href="https://github.com/itsecbern">
            <img src="https://avatars.githubusercontent.com/u/12274612?v=4" width="50;" alt="itsecbern"/>
            <br />
            <sub><b>Itsecbern</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/chriscpritchard">
            <img src="https://avatars.githubusercontent.com/u/1839074?v=4" width="50;" alt="chriscpritchard"/>
            <br />
            <sub><b>Chris Pritchard</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/vsc55">
            <img src="https://avatars.githubusercontent.com/u/13438676?v=4" width="50;" alt="vsc55"/>
            <br />
            <sub><b>Javier Pastor</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/sorano">
            <img src="https://avatars.githubusercontent.com/u/6185109?v=4" width="50;" alt="sorano"/>
            <br />
            <sub><b>Sorano</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/Vbgf">
            <img src="https://avatars.githubusercontent.com/u/5571734?v=4" width="50;" alt="Vbgf"/>
            <br />
            <sub><b>Vbgf</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/Qstick">
            <img src="https://avatars.githubusercontent.com/u/376117?v=4" width="50;" alt="Qstick"/>
            <br />
            <sub><b>Qstick</b></sub>
        </a>
    </td></tr>
<tr>
    <td align="center">
        <a href="https://github.com/MariusSchiffer">
            <img src="https://avatars.githubusercontent.com/u/183124?v=4" width="50;" alt="MariusSchiffer"/>
            <br />
            <sub><b>Marius Schiffer</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/jpeters">
            <img src="https://avatars.githubusercontent.com/u/167401?v=4" width="50;" alt="jpeters"/>
            <br />
            <sub><b>Jeffrey Peters</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/jackodsteel">
            <img src="https://avatars.githubusercontent.com/u/9209504?v=4" width="50;" alt="jackodsteel"/>
            <br />
            <sub><b>Jack Steel</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/Aerion">
            <img src="https://avatars.githubusercontent.com/u/9089317?v=4" width="50;" alt="Aerion"/>
            <br />
            <sub><b>Guillaume Taquet Gasperini</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/Fredrik81">
            <img src="https://avatars.githubusercontent.com/u/21292774?v=4" width="50;" alt="Fredrik81"/>
            <br />
            <sub><b>Fredrik81</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/pooley182">
            <img src="https://avatars.githubusercontent.com/u/5040011?v=4" width="50;" alt="pooley182"/>
            <br />
            <sub><b>David Pooley</b></sub>
        </a>
    </td></tr>
<tr>
    <td align="center">
        <a href="https://github.com/D34DC3N73R">
            <img src="https://avatars.githubusercontent.com/u/9123670?v=4" width="50;" alt="D34DC3N73R"/>
            <br />
            <sub><b>D34DC3N73R</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/au5ton">
            <img src="https://avatars.githubusercontent.com/u/4109551?v=4" width="50;" alt="au5ton"/>
            <br />
            <sub><b>Austin Jackson</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/andrewjmetzger">
            <img src="https://avatars.githubusercontent.com/u/590246?v=4" width="50;" alt="andrewjmetzger"/>
            <br />
            <sub><b>Andrew Metzger</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/bybeet">
            <img src="https://avatars.githubusercontent.com/u/1662279?v=4" width="50;" alt="bybeet"/>
            <br />
            <sub><b>Travis Bybee</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/Torkiliuz">
            <img src="https://avatars.githubusercontent.com/u/460764?v=4" width="50;" alt="Torkiliuz"/>
            <br />
            <sub><b>Torkil</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/tombomb">
            <img src="https://avatars.githubusercontent.com/u/544509?v=4" width="50;" alt="tombomb"/>
            <br />
            <sub><b>Tom McClellan</b></sub>
        </a>
    </td></tr>
<tr>
    <td align="center">
        <a href="https://github.com/Tim-Trott">
            <img src="https://avatars.githubusercontent.com/u/8249434?v=4" width="50;" alt="Tim-Trott"/>
            <br />
            <sub><b>Tim Trott</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/thomasvt1">
            <img src="https://avatars.githubusercontent.com/u/2271011?v=4" width="50;" alt="thomasvt1"/>
            <br />
            <sub><b>Thomas Van Tilburg</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/Teifun2">
            <img src="https://avatars.githubusercontent.com/u/7461832?v=4" width="50;" alt="Teifun2"/>
            <br />
            <sub><b>Teifun2</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/salexson">
            <img src="https://avatars.githubusercontent.com/u/33904499?v=4" width="50;" alt="salexson"/>
            <br />
            <sub><b>Steven Alexson</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/shoghicp">
            <img src="https://avatars.githubusercontent.com/u/516482?v=4" width="50;" alt="shoghicp"/>
            <br />
            <sub><b>Shoghi</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/seancallinan">
            <img src="https://avatars.githubusercontent.com/u/1139665?v=4" width="50;" alt="seancallinan"/>
            <br />
            <sub><b>Sean Callinan</b></sub>
        </a>
    </td></tr>
<tr>
    <td align="center">
        <a href="https://github.com/sambartik">
            <img src="https://avatars.githubusercontent.com/u/63553146?v=4" width="50;" alt="sambartik"/>
            <br />
            <sub><b>Samuel Bartík</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/rob1998">
            <img src="https://avatars.githubusercontent.com/u/1560707?v=4" width="50;" alt="rob1998"/>
            <br />
            <sub><b>Rob Gökemeijer</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/cqxmzz">
            <img src="https://avatars.githubusercontent.com/u/3071863?v=4" width="50;" alt="cqxmzz"/>
            <br />
            <sub><b>Qiming Chen</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/beast3334">
            <img src="https://avatars.githubusercontent.com/u/20631046?v=4" width="50;" alt="beast3334"/>
            <br />
            <sub><b>Nathan Miller</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/mvicomoya">
            <img src="https://avatars.githubusercontent.com/u/24613599?v=4" width="50;" alt="mvicomoya"/>
            <br />
            <sub><b>Miguel A Vico Moya</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/AliMickey">
            <img src="https://avatars.githubusercontent.com/u/60691199?v=4" width="50;" alt="AliMickey"/>
            <br />
            <sub><b>Micky</b></sub>
        </a>
    </td></tr>
<tr>
    <td align="center">
        <a href="https://github.com/LMaxence">
            <img src="https://avatars.githubusercontent.com/u/29194680?v=4" width="50;" alt="LMaxence"/>
            <br />
            <sub><b>Maxence Lecanu</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/mattmattmatt">
            <img src="https://avatars.githubusercontent.com/u/927830?v=4" width="50;" alt="mattmattmatt"/>
            <br />
            <sub><b>Matt</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/marleypowell">
            <img src="https://avatars.githubusercontent.com/u/55280588?v=4" width="50;" alt="marleypowell"/>
            <br />
            <sub><b>Marley</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/zobe123">
            <img src="https://avatars.githubusercontent.com/u/13840542?v=4" width="50;" alt="zobe123"/>
            <br />
            <sub><b>Zobe123</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/thegame3202">
            <img src="https://avatars.githubusercontent.com/u/22148848?v=4" width="50;" alt="thegame3202"/>
            <br />
            <sub><b>Mike</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/tdorsey">
            <img src="https://avatars.githubusercontent.com/u/1218404?v=4" width="50;" alt="tdorsey"/>
            <br />
            <sub><b>Tdorsey</b></sub>
        </a>
    </td></tr>
<tr>
    <td align="center">
        <a href="https://github.com/sir-marv">
            <img src="https://avatars.githubusercontent.com/u/3598205?v=4" width="50;" alt="sir-marv"/>
            <br />
            <sub><b>Sirmarv</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/ryan-c44">
            <img src="https://avatars.githubusercontent.com/u/54028283?v=4" width="50;" alt="ryan-c44"/>
            <br />
            <sub><b>Ryan-c44</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/mkgeeky">
            <img src="https://avatars.githubusercontent.com/u/68811367?v=4" width="50;" alt="mkgeeky"/>
            <br />
            <sub><b>Mkgeeky</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/masterhuck">
            <img src="https://avatars.githubusercontent.com/u/4671442?v=4" width="50;" alt="masterhuck"/>
            <br />
            <sub><b>Patrick Weber</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/malmcf">
            <img src="https://avatars.githubusercontent.com/u/243155210?v=4" width="50;" alt="malmcf"/>
            <br />
            <sub><b>malmcf</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/maartenheebink">
            <img src="https://avatars.githubusercontent.com/u/28894544?v=4" width="50;" alt="maartenheebink"/>
            <br />
            <sub><b>Maartenheebink</b></sub>
        </a>
    </td></tr>
<tr>
    <td align="center">
        <a href="https://github.com/m4tta">
            <img src="https://avatars.githubusercontent.com/u/427218?v=4" width="50;" alt="m4tta"/>
            <br />
            <sub><b>M4tta</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/echel0n">
            <img src="https://avatars.githubusercontent.com/u/1128022?v=4" width="50;" alt="echel0n"/>
            <br />
            <sub><b>Echel0n</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/baikunz">
            <img src="https://avatars.githubusercontent.com/u/984911?v=4" width="50;" alt="baikunz"/>
            <br />
            <sub><b>Dorian ALKOUM</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/distaula">
            <img src="https://avatars.githubusercontent.com/u/33949?v=4" width="50;" alt="distaula"/>
            <br />
            <sub><b>Michael DiStaula</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/devildant">
            <img src="https://avatars.githubusercontent.com/u/8700106?v=4" width="50;" alt="devildant"/>
            <br />
            <sub><b>Devildant</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/camjac251">
            <img src="https://avatars.githubusercontent.com/u/6313132?v=4" width="50;" alt="camjac251"/>
            <br />
            <sub><b>Camjac251</b></sub>
        </a>
    </td></tr>
<tr>
    <td align="center">
        <a href="https://github.com/bwittgen">
            <img src="https://avatars.githubusercontent.com/u/26683380?v=4" width="50;" alt="bwittgen"/>
            <br />
            <sub><b>Bwittgen</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/x-limitless-x">
            <img src="https://avatars.githubusercontent.com/u/17127926?v=4" width="50;" alt="x-limitless-x"/>
            <br />
            <sub><b>Blake Drumm</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/bazhip">
            <img src="https://avatars.githubusercontent.com/u/10350445?v=4" width="50;" alt="bazhip"/>
            <br />
            <sub><b>Tim OBrien</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/Xirg">
            <img src="https://avatars.githubusercontent.com/u/6020502?v=4" width="50;" alt="Xirg"/>
            <br />
            <sub><b>Xirg</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/Drewster727">
            <img src="https://avatars.githubusercontent.com/u/4528753?v=4" width="50;" alt="Drewster727"/>
            <br />
            <sub><b>Drew</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/onedr0p">
            <img src="https://avatars.githubusercontent.com/u/213795?v=4" width="50;" alt="onedr0p"/>
            <br />
            <sub><b>Devin Buhl</b></sub>
        </a>
    </td></tr>
<tr>
    <td align="center">
        <a href="https://github.com/davidtorosyan">
            <img src="https://avatars.githubusercontent.com/u/46736285?v=4" width="50;" alt="davidtorosyan"/>
            <br />
            <sub><b>David Torosyan</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/hmnd">
            <img src="https://avatars.githubusercontent.com/u/12853597?v=4" width="50;" alt="hmnd"/>
            <br />
            <sub><b>David</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/dben">
            <img src="https://avatars.githubusercontent.com/u/1358399?v=4" width="50;" alt="dben"/>
            <br />
            <sub><b>David Benson</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/danopia">
            <img src="https://avatars.githubusercontent.com/u/40628?v=4" width="50;" alt="danopia"/>
            <br />
            <sub><b>Daniel Lamando</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/Codehhh">
            <img src="https://avatars.githubusercontent.com/u/12055335?v=4" width="50;" alt="Codehhh"/>
            <br />
            <sub><b>Codehhh</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/cdemi">
            <img src="https://avatars.githubusercontent.com/u/8025435?v=4" width="50;" alt="cdemi"/>
            <br />
            <sub><b>Christopher Demicoli</b></sub>
        </a>
    </td></tr>
<tr>
    <td align="center">
        <a href="https://github.com/Crosenhain">
            <img src="https://avatars.githubusercontent.com/u/1342284?v=4" width="50;" alt="Crosenhain"/>
            <br />
            <sub><b>Chris</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/origamirobot">
            <img src="https://avatars.githubusercontent.com/u/1346803?v=4" width="50;" alt="origamirobot"/>
            <br />
            <sub><b>Chris Lees</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/CalvinWalzel">
            <img src="https://avatars.githubusercontent.com/u/6446452?v=4" width="50;" alt="CalvinWalzel"/>
            <br />
            <sub><b>Calvin</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/Majawat">
            <img src="https://avatars.githubusercontent.com/u/12058855?v=4" width="50;" alt="Majawat"/>
            <br />
            <sub><b>Majawat</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/Ashyni">
            <img src="https://avatars.githubusercontent.com/u/18462848?v=4" width="50;" alt="Ashyni"/>
            <br />
            <sub><b>Ashyni</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/Antonin-Bruzard">
            <img src="https://avatars.githubusercontent.com/u/82907030?v=4" width="50;" alt="Antonin-Bruzard"/>
            <br />
            <sub><b>Antonin</b></sub>
        </a>
    </td></tr>
<tr>
    <td align="center">
        <a href="https://github.com/alasano">
            <img src="https://avatars.githubusercontent.com/u/14372930?v=4" width="50;" alt="alasano"/>
            <br />
            <sub><b>Aljosa Asanovic</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/XanderStrike">
            <img src="https://avatars.githubusercontent.com/u/1565303?v=4" width="50;" alt="XanderStrike"/>
            <br />
            <sub><b>Alexander Standke</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/aj3x">
            <img src="https://avatars.githubusercontent.com/u/15078358?v=4" width="50;" alt="aj3x"/>
            <br />
            <sub><b>Alexander Russell</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/AbeKline">
            <img src="https://avatars.githubusercontent.com/u/8125653?v=4" width="50;" alt="AbeKline"/>
            <br />
            <sub><b>Abe Kline</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/Lucane">
            <img src="https://avatars.githubusercontent.com/u/7999446?v=4" width="50;" alt="Lucane"/>
            <br />
            <sub><b>Lucane</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/kmlucy">
            <img src="https://avatars.githubusercontent.com/u/13952475?v=4" width="50;" alt="kmlucy"/>
            <br />
            <sub><b>Kyle Lucy</b></sub>
        </a>
    </td></tr>
<tr>
    <td align="center">
        <a href="https://github.com/krisklosterman">
            <img src="https://avatars.githubusercontent.com/u/7139579?v=4" width="50;" alt="krisklosterman"/>
            <br />
            <sub><b>Kris Klosterman</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/jonocairns">
            <img src="https://avatars.githubusercontent.com/u/182836?v=4" width="50;" alt="jonocairns"/>
            <br />
            <sub><b>Jono Cairns</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/jonbloom">
            <img src="https://avatars.githubusercontent.com/u/492819?v=4" width="50;" alt="jonbloom"/>
            <br />
            <sub><b>Jon Bloom</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/ExtremeFiretop">
            <img src="https://avatars.githubusercontent.com/u/1971404?v=4" width="50;" alt="ExtremeFiretop"/>
            <br />
            <sub><b>Joel Samson</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/errorhandler">
            <img src="https://avatars.githubusercontent.com/u/17112958?v=4" width="50;" alt="errorhandler"/>
            <br />
            <sub><b>Joe Harvey</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/frebib">
            <img src="https://avatars.githubusercontent.com/u/775104?v=4" width="50;" alt="frebib"/>
            <br />
            <sub><b>Joe Groocock</b></sub>
        </a>
    </td></tr>
<tr>
    <td align="center">
        <a href="https://github.com/jamesmacwhite">
            <img src="https://avatars.githubusercontent.com/u/8067792?v=4" width="50;" alt="jamesmacwhite"/>
            <br />
            <sub><b>James White</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/JPyke3">
            <img src="https://avatars.githubusercontent.com/u/13283054?v=4" width="50;" alt="JPyke3"/>
            <br />
            <sub><b>Jacob Pyke</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/bommerts">
            <img src="https://avatars.githubusercontent.com/u/44821497?v=4" width="50;" alt="bommerts"/>
            <br />
            <sub><b>JGF</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/ImgBotApp">
            <img src="https://avatars.githubusercontent.com/u/31427850?v=4" width="50;" alt="ImgBotApp"/>
            <br />
            <sub><b>Imgbot</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/comigor">
            <img src="https://avatars.githubusercontent.com/u/735858?v=4" width="50;" alt="comigor"/>
            <br />
            <sub><b>Igor Borges</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/hariesramdhani">
            <img src="https://avatars.githubusercontent.com/u/24251244?v=4" width="50;" alt="hariesramdhani"/>
            <br />
            <sub><b>Haries Ramdhani</b></sub>
        </a>
    </td></tr>
<tr>
    <td align="center">
        <a href="https://github.com/ketsapiwiq">
            <img src="https://avatars.githubusercontent.com/u/26697460?v=4" width="50;" alt="ketsapiwiq"/>
            <br />
            <sub><b>Hadrien</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/Grygon">
            <img src="https://avatars.githubusercontent.com/u/647846?v=4" width="50;" alt="Grygon"/>
            <br />
            <sub><b>Grygon</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/Fish2">
            <img src="https://avatars.githubusercontent.com/u/2311734?v=4" width="50;" alt="Fish2"/>
            <br />
            <sub><b>Fish2</b></sub>
        </a>
    </td>
    <td align="center">
        <a href="https://github.com/elisspace">
            <img src="https://avatars.githubusercontent.com/u/18365129?v=4" width="50;" alt="elisspace"/>
            <br />
            <sub><b>Eli</b></sub>
        </a>
    </td></tr>
</table>
<!-- readme: collaborators,contributors -end -->

## Upstream credit and support

A massive thanks to Jamie Rees and every Ombi contributor whose work forms the foundation of this project.

If you want to support the **original Ombi project and its developer**, the upstream donation links are:

[![Patreon](https://img.shields.io/badge/patreon-support%20upstream-yellow.svg)](https://patreon.com/tidusjar/Ombi)
[![Paypal](https://img.shields.io/badge/paypal-support%20upstream-yellow.svg)](https://paypal.me/PlexRequestsNet)
