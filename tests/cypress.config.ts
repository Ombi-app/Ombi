import { defineConfig } from 'cypress';
import createBundler from "@bahmutov/cypress-esbuild-preprocessor";
import { addCucumberPreprocessorPlugin } from "@badeball/cypress-cucumber-preprocessor";
import createEsbuildPlugin from "@badeball/cypress-cucumber-preprocessor/esbuild";

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
    async setupNodeEvents(
      on: Cypress.PluginEvents,
      config: Cypress.PluginConfigOptions
    ): Promise<Cypress.PluginConfigOptions> {
      await addCucumberPreprocessorPlugin(on, config);

      on(
        "file:preprocessor",
        createBundler({
          plugins: [createEsbuildPlugin(config)],
        })
      );

      // Make sure to return the config object as it might have been modified by the plugin.
      return config;
      // return require('./cypress/plugins/index.js')(on, config)
    },
    baseUrl: 'http://localhost:5000',
    specPattern: ['cypress/tests/**/*.spec.ts*', '**/*.feature'],
    excludeSpecPattern: ['**/snapshots/*'],
  },
})
