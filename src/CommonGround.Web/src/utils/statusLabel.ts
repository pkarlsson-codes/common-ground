const STATUS_LABELS: Record<string, string> = {
  Available: 'Available',
  Occupied: 'Occupied',
  UnderDevelopment: 'Under Development',
  UnderRenovation: 'Under Renovation',
}

export function statusLabel(status: string): string {
  return STATUS_LABELS[status] ?? status
}
