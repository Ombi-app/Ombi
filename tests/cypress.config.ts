import { defineConfig } from 'cypress'

export default defineConfig({
  watchForFileChanges: true,
  chromeWebSecurity: false,
  viewportWidth: 2560,
  viewportHeight: 1440,
  retries: {
    runMode: 2,
    openMode: 0,
  },
  env: {
    username: 'a',
    password: 'a',
  },
  projectId: 'o5451s',
  e2e: {
    // We've imported your old cypress plugins here.
    // You may want to clean this up later by importing these.
    setupNodeEvents(on, config) {
      return require('./cypress/plugins/index.js')(on, config)
    },
    baseUrl: 'http://localhost:5000',
    specPattern: 'cypress/tests/**/*.spec.ts*',
    excludeSpecPattern: ['**/snapshots/*'],
  },
})
