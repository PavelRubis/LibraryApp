const allowedTags = new Set([
  'p',
  'div',
  'h1',
  'h2',
  'h3',
  'h4',
  'h5',
  'h6',
  'ul',
  'ol',
  'li',
  'strong',
  'b',
  'em',
  'i',
  'u',
  'br',
  'a',
  'span',
])

function isSafeUrl(value: string): boolean {
  return !/^\s*(javascript|data|vbscript):/i.test(value)
}

function copyNode(source: Node, targetDocument: XMLDocument): Node | null {
  if (source.nodeType === Node.TEXT_NODE) {
    return targetDocument.createTextNode(source.textContent ?? '')
  }

  if (source.nodeType !== Node.ELEMENT_NODE) return null

  const sourceElement = source as Element
  const tagName = sourceElement.tagName.toLowerCase()
  if (!allowedTags.has(tagName)) {
    const fragment = targetDocument.createDocumentFragment()
    sourceElement.childNodes.forEach((child) => {
      const copied = copyNode(child, targetDocument)
      if (copied) fragment.appendChild(copied)
    })
    return fragment
  }

  const target = targetDocument.createElement(tagName)
  if (tagName === 'a') {
    const href = sourceElement.getAttribute('href')
    if (href && isSafeUrl(href)) target.setAttribute('href', href)
  }

  sourceElement.childNodes.forEach((child) => {
    const copied = copyNode(child, targetDocument)
    if (copied) target.appendChild(copied)
  })

  return target
}

export function htmlToTocXml(html: string): string {
  const htmlDocument = new DOMParser().parseFromString(html, 'text/html')
  const xmlDocument = document.implementation.createDocument(null, 'toc', null)
  const root = xmlDocument.documentElement

  htmlDocument.body.childNodes.forEach((child) => {
    const copied = copyNode(child, xmlDocument)
    if (copied) root.appendChild(copied)
  })

  return new XMLSerializer().serializeToString(xmlDocument)
}

export function validateTocXml(xml: string | null | undefined): string | true {
  if (!xml?.trim()) return 'Добавьте оглавление.'
  if (xml.length > 1024 * 1024) return 'Оглавление не должно превышать 1 МБ.'

  const document = new DOMParser().parseFromString(xml, 'application/xml')
  if (document.querySelector('parsererror')) return 'Оглавление содержит некорректный XML.'
  if (document.documentElement.localName !== 'toc') return 'Корневой элемент должен быть <toc>.'
  if (!document.documentElement.textContent?.trim()) return 'Добавьте хотя бы один пункт оглавления.'

  return true
}

export function tocXmlToHtml(xml: string | null | undefined): string {
  if (!xml) return ''

  const xmlDocument = new DOMParser().parseFromString(xml, 'application/xml')
  if (xmlDocument.querySelector('parsererror') || xmlDocument.documentElement.localName !== 'toc') {
    return ''
  }

  const htmlDocument = document.implementation.createHTMLDocument('')
  const container = htmlDocument.createElement('div')
  xmlDocument.documentElement.childNodes.forEach((child) => {
    const copied = copyNode(child, htmlDocument as unknown as XMLDocument)
    if (copied) container.appendChild(copied)
  })

  return container.innerHTML
}
