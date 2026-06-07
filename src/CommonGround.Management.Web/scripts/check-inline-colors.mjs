#!/usr/bin/env node
import fs from 'node:fs'
import path from 'node:path'

const ROOT = path.resolve(process.cwd(), 'src')
const TARGET_EXTENSIONS = new Set(['.ts', '.tsx'])
const IGNORE_DIRS = new Set(['node_modules', 'dist', 'build'])

const COLOR_LITERAL_RE = /#(?:[0-9a-f]{3,8})\b|\b(?:rgb|rgba|hsl|hsla)\(/i
const CSS_VAR_RE = /\bvar\(--[a-z0-9-]+\)/i
const COLOR_MIX_RE = /\bcolor-mix\(/i
const ALLOWED_COLOR_KEYWORD_RE = /^(?:transparent|currentcolor|inherit|initial|unset)$/i

const COLOR_KEYS = [
  'color',
  'background',
  'backgroundColor',
  'border',
  'borderTop',
  'borderRight',
  'borderBottom',
  'borderLeft',
  'borderColor',
  'outline',
  'outlineColor',
  'fill',
  'stroke',
  'boxShadow',
  'textShadow',
]

const STYLE_STRING_RE = /style\s*=\s*(?:"([^"]*)"|'([^']*)'|\{\s*`([^`]*)`\s*\}|\{\s*"([^"]*)"\s*\}|\{\s*'([^']*)'\s*\})/g
const STYLE_OBJECT_RE = /style\s*=\s*\{\s*\{([\s\S]*?)\}\s*\}/g

function isDisallowedColorLiteral(value) {
  const trimmed = value.trim()
  if (!trimmed) {
    return false
  }

  if (ALLOWED_COLOR_KEYWORD_RE.test(trimmed)) {
    return false
  }

  if (CSS_VAR_RE.test(trimmed) || COLOR_MIX_RE.test(trimmed)) {
    return false
  }

  return COLOR_LITERAL_RE.test(trimmed)
}

function getLineAndColumn(source, index) {
  const upToMatch = source.slice(0, index)
  const lines = upToMatch.split('\n')
  const line = lines.length
  const column = lines[lines.length - 1].length + 1
  return { line, column }
}

function collectFiles(directory, files = []) {
  const entries = fs.readdirSync(directory, { withFileTypes: true })

  for (const entry of entries) {
    if (entry.isDirectory()) {
      if (IGNORE_DIRS.has(entry.name)) {
        continue
      }
      collectFiles(path.join(directory, entry.name), files)
      continue
    }

    if (!entry.isFile()) {
      continue
    }

    const ext = path.extname(entry.name)
    if (TARGET_EXTENSIONS.has(ext)) {
      files.push(path.join(directory, entry.name))
    }
  }

  return files
}

function checkStyleString(filePath, source, block, indexOffset, failures) {
  const declarations = block.split(';')

  for (const declaration of declarations) {
    const colonIndex = declaration.indexOf(':')
    if (colonIndex === -1) {
      continue
    }

    const rawProperty = declaration.slice(0, colonIndex).trim()
    const rawValue = declaration.slice(colonIndex + 1).trim()
    if (!rawProperty || !rawValue) {
      continue
    }

    const camelProperty = rawProperty
      .toLowerCase()
      .replace(/-([a-z])/g, (_, char) => char.toUpperCase())
    if (!COLOR_KEYS.includes(camelProperty)) {
      continue
    }

    if (!isDisallowedColorLiteral(rawValue)) {
      continue
    }

    const coordinates = getLineAndColumn(source, indexOffset)
    failures.push({
      filePath,
      line: coordinates.line,
      column: coordinates.column,
      detail: `style string uses raw color literal for ${rawProperty}`,
    })
  }
}

function checkStyleObject(filePath, source, objectContent, indexOffset, failures) {
  for (const key of COLOR_KEYS) {
    const keyPattern = new RegExp(`(?:^|[\\s,{])${key}\\s*:\\s*([^,}]+)`, 'gi')
    let keyMatch

    while ((keyMatch = keyPattern.exec(objectContent)) !== null) {
      const rawValue = keyMatch[1].trim()
      if (!isDisallowedColorLiteral(rawValue)) {
        continue
      }

      const coordinates = getLineAndColumn(source, indexOffset + keyMatch.index)
      failures.push({
        filePath,
        line: coordinates.line,
        column: coordinates.column,
        detail: `inline style object uses raw color literal for ${key}`,
      })
    }
  }
}

function checkFile(filePath, failures) {
  const source = fs.readFileSync(filePath, 'utf8')

  let match
  while ((match = STYLE_STRING_RE.exec(source)) !== null) {
    const styleText = match[1] ?? match[2] ?? match[3] ?? match[4] ?? match[5] ?? ''
    checkStyleString(filePath, source, styleText, match.index, failures)
  }

  while ((match = STYLE_OBJECT_RE.exec(source)) !== null) {
    const objectContent = match[1] ?? ''
    checkStyleObject(filePath, source, objectContent, match.index, failures)
  }
}

function main() {
  if (!fs.existsSync(ROOT)) {
    process.stdout.write('No src directory found, skipping inline color check.\n')
    process.exit(0)
  }

  const files = collectFiles(ROOT)
  const failures = []

  for (const filePath of files) {
    checkFile(filePath, failures)
  }

  if (failures.length === 0) {
    process.stdout.write('Inline color guard: no raw color literals found in style props.\n')
    return
  }

  process.stdout.write('Inline color guard violations:\n')
  for (const failure of failures) {
    const relative = path.relative(process.cwd(), failure.filePath)
    process.stdout.write(`- ${relative}:${failure.line}:${failure.column} ${failure.detail}\n`)
  }

  process.exit(1)
}

main()
