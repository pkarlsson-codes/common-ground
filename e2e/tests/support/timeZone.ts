/**
 * The browser's time zone in every test, so the trust auth.setup creates gets it too. Not UTC, so a
 * date taken from the UTC clock instead of the trust's shows up as off by one in the evening.
 */
export const E2E_TIME_ZONE = 'America/Denver'
