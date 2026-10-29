// @ts-nocheck
import { parseCalendarDate, type CalendarDate } from '../../lib/calendarDate'

export type { CalendarDate }

export function CalendarDateFromJSON(json: any): CalendarDate {
    return parseCalendarDate(json)
}

export function CalendarDateFromJSONTyped(json: any, _ignoreDiscriminator: boolean): CalendarDate {
    return parseCalendarDate(json)
}

export function CalendarDateToJSON(value?: CalendarDate | null): any {
    return value
}

export function CalendarDateToJSONTyped(value?: CalendarDate | null, _ignoreDiscriminator: boolean = false): any {
    return value
}
