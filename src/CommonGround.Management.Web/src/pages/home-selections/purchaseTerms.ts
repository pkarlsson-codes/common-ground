import { trustToday, type CalendarDate } from '../../lib/calendarDate'

/** The terms the purchase workflow starts with once a household is chosen. */
export type PurchaseTerms = {
  groundLeaseTemplateId: string
  purchasePrice: number
  selectedDate: CalendarDate
}

export const emptyPurchaseTerms = (): PurchaseTerms => ({
  groundLeaseTemplateId: '',
  purchasePrice: 0,
  selectedDate: trustToday(),
})

export const purchaseTermsComplete = (terms: PurchaseTerms) =>
  !!terms.groundLeaseTemplateId && !!terms.purchasePrice && !!terms.selectedDate
