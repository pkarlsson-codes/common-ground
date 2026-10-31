import { useCallback, useState } from 'react'
import { isFocus, type Focus } from '../lib/navigation'

const FOCUS_KEY = 'cg.nav.focus'
const OTHER_WORK_OPEN_KEY = 'cg.nav.otherWorkOpen'

// Browser storage can be missing, blocked or full; a preference that can't be
// read falls back to its default and one that can't be written is just not kept.
function read(key: string): string | null {
  try {
    return window.localStorage.getItem(key)
  } catch {
    return null
  }
}

function write(key: string, value: string) {
  try {
    window.localStorage.setItem(key, value)
  } catch {
    // not kept
  }
}

export function readFocus(): Focus {
  const value = read(FOCUS_KEY)
  return isFocus(value) ? value : 'everything'
}

export function readOtherWorkOpen(): boolean {
  return read(OTHER_WORK_OPEN_KEY) === 'true'
}

/** The staff member's work focus and whether Other work is expanded, kept in this browser. */
export function useNavPreferences() {
  const [focus, setFocusState] = useState<Focus>(readFocus)
  const [otherWorkOpen, setOtherWorkOpenState] = useState(readOtherWorkOpen)

  const setFocus = useCallback((value: Focus) => {
    setFocusState(value)
    write(FOCUS_KEY, value)
  }, [])

  const setOtherWorkOpen = useCallback((value: boolean) => {
    setOtherWorkOpenState(value)
    write(OTHER_WORK_OPEN_KEY, String(value))
  }, [])

  return { focus, setFocus, otherWorkOpen, setOtherWorkOpen }
}
