import { createVuetify } from 'vuetify'
import * as components from 'vuetify/components'
import * as directives from 'vuetify/directives'

export default createVuetify({
  components,
  directives,
  theme: {
    defaultTheme: 'libraryTheme',
    themes: {
      libraryTheme: {
        dark: false,
        colors: {
          background: '#f5f2ea',
          surface: '#fffdf8',
          primary: '#245b52',
          secondary: '#b56a3b',
          accent: '#d9a441',
          error: '#b3261e',
          info: '#3f6f83',
          success: '#34785a',
        },
      },
    },
  },
  defaults: {
    VBtn: { rounded: 'lg' },
    VCard: { rounded: 'xl' },
    VTextField: { variant: 'outlined', density: 'comfortable' },
    VAutocomplete: { variant: 'outlined', density: 'comfortable' },
  },
})
