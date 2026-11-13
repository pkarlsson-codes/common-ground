import { describe, expect, it } from 'vitest'
import { toCsv } from './csv'

describe('toCsv', () => {
  it('writes a header and rows with CRLF line ends', () => {
    expect(toCsv(['Home', 'Price'], [['12 Oak St', 180000]])).toBe('Home,Price\r\n12 Oak St,180000\r\n')
  })

  it('leaves null and undefined cells empty', () => {
    expect(toCsv(['A', 'B', 'C'], [[null, undefined, 0]])).toBe('A,B,C\r\n,,0\r\n')
  })

  it('quotes cells with commas, quotes or line breaks', () => {
    expect(toCsv(['Home'], [['Unit 4, "Elm"'], ['a\nb']])).toBe('Home\r\n"Unit 4, ""Elm"""\r\n"a\nb"\r\n')
  })
})
