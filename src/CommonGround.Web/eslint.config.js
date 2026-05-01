import js from '@eslint/js'
import globals from 'globals'
import reactHooks from 'eslint-plugin-react-hooks'
import reactRefresh from 'eslint-plugin-react-refresh'
import tseslint from 'typescript-eslint'
import { defineConfig, globalIgnores } from 'eslint/config'

const INLINE_COLOR_KEYS = new Set([
  'color',
  'background',
  'backgroundColor',
  'borderColor',
  'outlineColor',
  'fill',
  'stroke',
  'boxShadow',
  'textShadow',
  'shadow',
])

const INLINE_BORDER_KEYS = new Set([
  'border',
  'borderTop',
  'borderRight',
  'borderBottom',
  'borderLeft',
  'outline',
])

const CSS_VAR_RE = /\bvar\(--[a-z0-9-]+\)/i
const COLOR_MIX_RE = /\bcolor-mix\(/i
const COLOR_LITERAL_RE = /#(?:[0-9a-f]{3,8})\b|\b(?:rgb|rgba|hsl|hsla)\(/i
const ALLOWED_COLOR_KEYWORD_RE = /^(?:transparent|currentcolor|inherit|initial|unset)$/i

const tokenizedColorsPlugin = {
  rules: {
    'no-raw-inline-colors': {
      meta: {
        type: 'problem',
        docs: {
          description: 'Disallow raw color literals in JSX style objects for color-bearing properties.',
        },
        schema: [],
        messages: {
          useToken: 'Use a design token via var(--...) for inline color styles (raw color literals are not allowed).',
        },
      },
      create(context) {
        const isColorProperty = (name) => INLINE_COLOR_KEYS.has(name) || INLINE_BORDER_KEYS.has(name)

        const getPropertyName = (property) => {
          if (property.computed) {
            return null
          }

          if (property.key.type === 'Identifier') {
            return property.key.name
          }

          if (property.key.type === 'Literal' && typeof property.key.value === 'string') {
            return property.key.value
          }

          return null
        }

        const getStaticString = (node) => {
          if (!node) {
            return null
          }

          if (node.type === 'Literal' && typeof node.value === 'string') {
            return node.value
          }

          if (node.type === 'TemplateLiteral' && node.expressions.length === 0) {
            return node.quasis.map((q) => q.value.cooked ?? '').join('')
          }

          return null
        }

        const isDisallowedColorLiteral = (value) => {
          const trimmed = value.trim()

          if (ALLOWED_COLOR_KEYWORD_RE.test(trimmed)) {
            return false
          }

          if (CSS_VAR_RE.test(trimmed) || COLOR_MIX_RE.test(trimmed)) {
            return false
          }

          return COLOR_LITERAL_RE.test(trimmed)
        }

        const checkStyleString = (node, styleText) => {
          if (typeof styleText !== 'string') {
            return
          }

          const declarations = styleText.split(';')
          for (const declaration of declarations) {
            const colonIndex = declaration.indexOf(':')
            if (colonIndex === -1) {
              continue
            }

            const rawProperty = declaration.slice(0, colonIndex).trim().toLowerCase()
            const rawValue = declaration.slice(colonIndex + 1).trim()
            if (!rawProperty || !rawValue) {
              continue
            }

            const camelProperty = rawProperty.replace(/-([a-z])/g, (_, char) => char.toUpperCase())
            if (!isColorProperty(camelProperty)) {
              continue
            }

            if (isDisallowedColorLiteral(rawValue)) {
              context.report({ node, messageId: 'useToken' })
            }
          }
        }

        const checkValueNode = (node) => {
          if (!node) {
            return
          }

          const staticString = getStaticString(node)
          if (typeof staticString === 'string') {
            if (isDisallowedColorLiteral(staticString)) {
              context.report({ node, messageId: 'useToken' })
            }
            return
          }

          if (node.type === 'ConditionalExpression') {
            checkValueNode(node.consequent)
            checkValueNode(node.alternate)
            return
          }

          if (node.type === 'LogicalExpression') {
            checkValueNode(node.left)
            checkValueNode(node.right)
          }
        }

        const checkStyleObject = (objectNode) => {
          for (const property of objectNode.properties) {
            if (!property || property.type !== 'Property') {
              continue
            }

            const propertyName = getPropertyName(property)
            if (!propertyName || !isColorProperty(propertyName)) {
              continue
            }

            checkValueNode(property.value)
          }
        }

        return {
          JSXAttribute(node) {
            if (node.name?.type !== 'JSXIdentifier' || node.name.name !== 'style') {
              return
            }

            if (!node.value) {
              return
            }

            if (node.value.type === 'Literal' && typeof node.value.value === 'string') {
              checkStyleString(node.value, node.value.value)
              return
            }

            if (node.value.type !== 'JSXExpressionContainer') {
              return
            }

            const expression = node.value.expression
            if (!expression) {
              return
            }

            const expressionString = getStaticString(expression)
            if (typeof expressionString === 'string') {
              checkStyleString(expression, expressionString)
              return
            }

            if (expression.type !== 'ObjectExpression') {
              return
            }

            checkStyleObject(expression)
          },
        }
      },
    },
  },
}

export default defineConfig([
  globalIgnores(['dist']),
  {
    files: ['**/*.{ts,tsx}'],
    plugins: {
      'tokenized-colors': tokenizedColorsPlugin,
    },
    extends: [
      js.configs.recommended,
      tseslint.configs.recommended,
      reactHooks.configs.flat.recommended,
      reactRefresh.configs.vite,
    ],
    rules: {
      'tokenized-colors/no-raw-inline-colors': 'error',
    },
    languageOptions: {
      ecmaVersion: 2020,
      globals: globals.browser,
    },
  },
])
