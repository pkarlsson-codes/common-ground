// Browser storage can be missing, blocked or full; a preference that can't be
// read falls back to its default and one that can't be written is just not kept.
export function readPreference(key: string): string | null {
  try {
    return window.localStorage.getItem(key)
  } catch {
    return null
  }
}

export function writePreference(key: string, value: string) {
  try {
    window.localStorage.setItem(key, value)
  } catch {
    // not kept
  }
}
