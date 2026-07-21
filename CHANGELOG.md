# Changelog

## v1.0.0

Initial release.

- Stubborn assault (on by default): attackers no longer pull back the moment they
  cannot get up the walls. Vanilla ties the retreat threshold to assault progress,
  which makes stalled sieges rock back and forth indefinitely. Changes AI
  behaviour, stated first in the description, and can be switched off.
- Fix: besieging army wiped out (captured/killed, lords made fugitive) after a
  failed assault, even when it had healthy survivors.
- Guard: vanilla NullReferenceException in `MobileParty.OnPartyJoinedSiegeInternal`
  hard-crashing the game during the campaign tick.
- All three toggleable in MCM.
