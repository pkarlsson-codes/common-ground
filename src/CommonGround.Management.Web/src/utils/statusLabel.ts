const STATUS_LABELS: Record<string, string> = {
  Available: 'Available',
  Predevelopment: 'Predevelopment',
  UnderConstruction: 'Under Construction',
  ReadyForOccupancy: 'Ready for Occupancy',
  Transitioning: 'Transitioning',
  Decommissioned: 'Decommissioned',
  Occupied: 'Occupied',
  HeldByTrust: 'Held by Trust',
  PassedOver: 'Passed Over',
  CouldNotBeChosen: "Couldn't Be Chosen",
  UnderDevelopment: 'Under Development',
  UnderRenovation: 'Under Renovation',
}

export function statusLabel(status: string): string {
  return STATUS_LABELS[status] ?? status.replace(/([a-z])([A-Z])/g, '$1 $2')
}
