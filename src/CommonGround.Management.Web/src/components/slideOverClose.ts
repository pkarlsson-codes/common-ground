import { createContext, useContext } from 'react'

// SlideOver provides this so a footer's Cancel button asks "Discard changes?" when the form is dirty.
export const SlideOverCloseContext = createContext<(() => void) | null>(null)

export function useSlideOverClose(): () => void {
  const close = useContext(SlideOverCloseContext)
  if (!close) throw new Error('useSlideOverClose must be used inside a SlideOver')
  return close
}
