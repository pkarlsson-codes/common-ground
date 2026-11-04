import { useCallback, useState } from 'react'
import { isFocus, type Focus } from '../lib/navigation'
import { readPreference as read, writePreference as write } from '../lib/preferences'

const FOCUS_KEY = 'cg.nav.focus'
const OTHER_WORK_OPEN_KEY = 'cg.nav.otherWorkOpen'

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
