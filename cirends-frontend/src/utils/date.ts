export function toUtcIso(dateLike: string | Date | null | undefined): string | null {
  if (!dateLike) return null
  const d = dateLike instanceof Date ? dateLike : new Date(dateLike)
  if (isNaN(d.getTime())) return null
  return d.toISOString()
}

/**
 * Konvertuoja ISO datą į datetime-local formato stringą (be Z, be ms)
 * @param isoDate - ISO formato data (pvz.: "2025-11-27T11:34:00Z")
 * @returns datetime-local formatas (pvz.: "2025-11-27T11:34")
 */
export function toDateTimeLocal(isoDate: string | null | undefined): string {
  if (!isoDate) return ''
  try {
    const d = new Date(isoDate)
    if (isNaN(d.getTime())) return ''
    // Get local time components
    const year = d.getFullYear()
    const month = String(d.getMonth() + 1).padStart(2, '0')
    const day = String(d.getDate()).padStart(2, '0')
    const hours = String(d.getHours()).padStart(2, '0')
    const minutes = String(d.getMinutes()).padStart(2, '0')
    return `${year}-${month}-${day}T${hours}:${minutes}`
  } catch {
    return ''
  }
}
/**
 * Formatuoja datą į lietuvišką formatą
 * @param date - Data kaip string arba Date objektas
 * @returns Suformatuota data (pvz.: "2024-11-22")
 */
export function formatDate(date: string | Date | null | undefined): string {
  if (!date) return ''
  
  try {
    const d = new Date(date)
    if (isNaN(d.getTime())) return ''
    
    return d.toLocaleDateString('lt-LT', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit'
    })
  } catch {
    return ''
  }
}

/**
 * Formatuoja datą ir laiką į lietuvišką formatą
 * @param date - Data kaip string arba Date objektas
 * @returns Suformatuota data ir laikas (pvz.: "2024-11-22 14:30")
 */
export function formatDateTime(date: string | Date | null | undefined): string {
  if (!date) return ''
  
  try {
    const d = new Date(date)
    if (isNaN(d.getTime())) return ''
    
    return d.toLocaleString('lt-LT', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit'
    })
  } catch {
    return ''
  }
}

/**
 * Formatuoja datą į pilną lietuvišką formatą
 * @param date - Data kaip string arba Date objektas
 * @returns Suformatuota data (pvz.: "2024 m. lapkričio 22 d.")
 */
export function formatDateLong(date: string | Date | null | undefined): string {
  if (!date) return ''
  
  try {
    const d = new Date(date)
    if (isNaN(d.getTime())) return ''
    
    return d.toLocaleDateString('lt-LT', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    })
  } catch {
    return ''
  }
}

/**
 * Formatuoja datą į santykinį formatą (pvz.: "prieš 2 dienas")
 * @param date - Data kaip string arba Date objektas
 * @returns Santykinė data
 */
export function formatRelativeTime(date: string | Date | null | undefined): string {
  if (!date) return ''
  
  try {
    const d = new Date(date)
    if (isNaN(d.getTime())) return ''
    
    const now = new Date()
    const diffMs = now.getTime() - d.getTime()
    const diffSec = Math.floor(diffMs / 1000)
    const diffMin = Math.floor(diffSec / 60)
    const diffHour = Math.floor(diffMin / 60)
    const diffDay = Math.floor(diffHour / 24)
    
    if (diffSec < 60) return 'ką tik'
    if (diffMin < 60) return `prieš ${diffMin} min.`
    if (diffHour < 24) return `prieš ${diffHour} val.`
    if (diffDay < 7) return `prieš ${diffDay} d.`
    if (diffDay < 30) return `prieš ${Math.floor(diffDay / 7)} sav.`
    if (diffDay < 365) return `prieš ${Math.floor(diffDay / 30)} mėn.`
    return `prieš ${Math.floor(diffDay / 365)} m.`
  } catch {
    return ''
  }
}

/**
 * Tikrina ar data yra praėjusi
 * @param date - Data kaip string arba Date objektas
 * @returns true jei data praėjusi, false jei ne
 */
export function isExpired(date: string | Date | null | undefined): boolean {
  if (!date) return false
  
  try {
    const d = new Date(date)
    if (isNaN(d.getTime())) return false
    
    return d < new Date()
  } catch {
    return false
  }
}

/**
 * Tikrina ar data yra ateityje
 * @param date - Data kaip string arba Date objektas
 * @returns true jei data ateityje, false jei ne
 */
export function isFuture(date: string | Date | null | undefined): boolean {
  if (!date) return false
  
  try {
    const d = new Date(date)
    if (isNaN(d.getTime())) return false
    
    return d > new Date()
  } catch {
    return false
  }
}

/**
 * Patikrina, ar veikla vyksta dabar (prasidėjo, bet dar nesibaigė)
 */
export function isActive(startDate: string | Date | null | undefined, endDate: string | Date | null | undefined): boolean {
  if (!startDate) return false
  const now = Date.now()
  const start = new Date(startDate).getTime()
  
  if (start > now) return false // dar neprasidėjo
  
  if (!endDate) return true // prasidėjo ir nėra pabaigos datos
  
  const end = new Date(endDate).getTime()
  return now >= start && now <= end
}

/**
 * Formatuoja datą į ISO formatą (YYYY-MM-DD)
 * Naudojama formose
 */
export function toISODate(date: Date | string | null | undefined): string {
  if (!date) return ''
  
  try {
    const d = typeof date === 'string' ? new Date(date) : date
    if (isNaN(d.getTime())) return ''
    
    const isoString = d.toISOString().split('T')[0]
    return isoString || ''
  } catch {
    return ''
  }
}

/**
 * Skaičiuoja dienų skaičių tarp dviejų datų
 */
export function daysBetween(start: string | Date, end: string | Date): number {
  try {
    const startDate = new Date(start)
    const endDate = new Date(end)
    
    if (isNaN(startDate.getTime()) || isNaN(endDate.getTime())) return 0
    
    const diffMs = endDate.getTime() - startDate.getTime()
    return Math.floor(diffMs / (1000 * 60 * 60 * 24))
  } catch {
    return 0
  }
}
