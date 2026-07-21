# Changelog

## v1.0.0

Initial release.

- Fix: besieging army wiped out (captured/killed, lords made fugitive) after a
  failed assault, even when it had healthy survivors.
- Guard: vanilla NullReferenceException in `MobileParty.OnPartyJoinedSiegeInternal`
  hard-crashing the game during the campaign tick.
- Optional (off by default): stubborn assault — attackers no longer pull back the
  moment they cannot get up the walls.
- All three toggleable in MCM.
