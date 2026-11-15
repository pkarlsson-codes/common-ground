import type { HeirTransferResponse } from '../../api'
import { formatCalendarDate, longDate, type CalendarDate } from '../../lib/calendarDate'

export interface HeirTransferLetter {
  date: string
  recipient: string[]
  heading: string
  greeting: string
  paragraphs: string[]
  signOff: string[]
}

/** Only a completed transfer has a letter. */
export const heirLetterApplies = (status: string | undefined) => status === 'Completed'

/** The trust's consent to the lease passing to the heir (Art 10.3), written once the signed letters are on file. */
export function heirTransferLetter(
  t: HeirTransferResponse,
  homeLabel: string | null,
  today: CalendarDate,
  signOff: string,
): HeirTransferLetter {
  const heir = t.heirFullName ?? 'Heir'
  const deceased = t.deceasedHomeownerFullName ?? 'the former homeowner'
  const home = homeLabel ?? 'the home'
  const effective = t.transferredOn ? ` effective ${formatCalendarDate(t.transferredOn, longDate)}` : ''

  return {
    date: formatCalendarDate(today, longDate),
    recipient: [heir, ...(homeLabel ? [homeLabel] : [])],
    heading: 'Consent to transfer of your home',
    greeting: `Dear ${heir},`,
    paragraphs: [
      `The trust consents under Art 10.3 of the ground lease to the transfer of ${home} from ${deceased} to you${effective}.`,
      'We hold your signed Letter of Agreement and your attorney\'s signed Acknowledgment with the lease. '
        + 'The lease continues on its existing terms and the resale price formula is unchanged.',
    ],
    signOff: signOff.split('\n').map(l => l.trim()).filter(Boolean),
  }
}

/** Opens a completed transfer's letter in a new tab, where the print dialog opens. */
export const openHeirTransferLetter = (heirTransferId: string) =>
  window.open(`/heir-transfers/${encodeURIComponent(heirTransferId)}/letter`, '_blank')
