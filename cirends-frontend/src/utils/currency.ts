/**
 * Formatuoja skaičių į pinigų formatą su euro ženklu
 * @param amount - Suma
 * @returns Suformatuota suma (pvz.: "50.00 €")
 */
export function formatCurrency(amount: number | null | undefined): string {
  if (amount === null || amount === undefined) return '0.00 €'
  
  try {
    return `${amount.toFixed(2)} €`
  } catch {
    return '0.00 €'
  }
}

/**
 * Formatuoja skaičių į pinigų formatą su tūkstančių skyrikliais
 * @param amount - Suma
 * @returns Suformatuota suma (pvz.: "1,234.56 €")
 */
export function formatCurrencyWithSeparators(amount: number | null | undefined): string {
  if (amount === null || amount === undefined) return '0.00 €'
  
  try {
    return amount.toLocaleString('lt-LT', {
      style: 'currency',
      currency: 'EUR',
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    })
  } catch {
    return '0.00 €'
  }
}

/**
 * Parsina string'ą į skaičių (pinigų suma)
 * @param value - String reikšmė
 * @returns Skaičius
 */
export function parseCurrency(value: string | null | undefined): number {
  if (!value) return 0
  
  try {
    // Pašalina visus simbolius išskyrus skaičius, minusą ir tašką
    const cleaned = value.replace(/[^\d.-]/g, '')
    const parsed = parseFloat(cleaned)
    
    return isNaN(parsed) ? 0 : parsed
  } catch {
    return 0
  }
}

/**
 * Patikrina ar suma yra teigiama
 */
export function isPositiveAmount(amount: number | null | undefined): boolean {
  return amount !== null && amount !== undefined && amount > 0
}

/**
 * Patikrina ar suma yra neigiama
 */
export function isNegativeAmount(amount: number | null | undefined): boolean {
  return amount !== null && amount !== undefined && amount < 0
}

/**
 * Suapvalina sumą iki centų
 */
export function roundToTwoDecimals(amount: number): number {
  return Math.round(amount * 100) / 100
}

/**
 * Skaičiuoja procentą
 * @param amount - Suma
 * @param total - Bendra suma
 * @returns Procentas (0-100)
 */
export function calculatePercentage(amount: number, total: number): number {
  if (total === 0) return 0
  return roundToTwoDecimals((amount / total) * 100)
}

/**
 * Skaičiuoja sumą pagal procentą
 * @param total - Bendra suma
 * @param percentage - Procentas (0-100)
 * @returns Apskaičiuota suma
 */
export function calculateAmountFromPercentage(total: number, percentage: number): number {
  return roundToTwoDecimals((total * percentage) / 100)
}

/**
 * Formatuoja balansą (skolą arba permoką)
 * Neigiama reikšmė = skola (raudona)
 * Teigiama reikšmė = permoka (žalia)
 */
export function formatBalance(balance: number): string {
  const formatted = formatCurrency(Math.abs(balance))
  
  if (balance < 0) {
    return `Skola: ${formatted}`
  } else if (balance > 0) {
    return `Permoka: ${formatted}`
  } else {
    return 'Subalansuota'
  }
}

/**
 * Grąžina spalvos klasę pagal balansą
 */
export function getBalanceColorClass(balance: number): string {
  if (balance < 0) return 'text-red-600'
  if (balance > 0) return 'text-green-600'
  return 'text-gray-600'
}
