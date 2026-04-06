import { defineConfig } from 'cypress';
import createBundler from "@bahmutov/cypress-esbuild-preprocessor";
import { addCucumberPreprocessorPlugin } from "@badeball/cypress-cucumber-preprocessor";
import createEsbuildPlugin from "@badeball/cypress-cucumber-preprocessor/esbuild";

export default defineConfig({
  // Performance optimizations
  watchForFileChanges: false, // Disable in CI
  video: false, // Disable video recording for faster runs
  screenshotOnRunFailure: true,
  trashAssetsBeforeRuns: true,
  
  // Security and viewport
  chromeWebSecurity: false,
  viewportWidth: 1920,
  viewportHeight: 1080,
  
  // Retry configuration
  retries: {
    runMode: 2,
    openMode: 0,
  },
  
  // Environment variables
  env: {
    username: 'a',
    password: 'a',
    dockerhost: 'http://172.17.0.1',
    // Add test environment flags
    isCI: process.env.CI === 'true',
    // Add API base URL
    apiBaseUrl: 'http://localhost:3577/api/v1'
  },
  
  // Project configuration - can be overridden via CYPRESS_PROJECT_ID env var for multi-db CI
  projectId: process.env.CYPRESS_PROJECT_ID || 'o5451s',
  
  e2e: {
    // Setup node events
    async setupNodeEvents(
      on: Cypress.PluginEvents,
      config: Cypress.PluginConfigOptions
    ): Promise<Cypress.PluginConfigOptions> {
      await addCucumberPreprocessorPlugin(on, config);

      on(
        "file:preprocessor",
        createBundler({
          plugins: [createEsbuildPlugin(config) as any],
        })
      );

      // Add performance monitoring
      on('task', {
        log(message) {
          console.log(message);
          return null;
        },
        table(message) {
          console.table(message);
          return null;
        }
      });

      return config;
    },
    
    // Base configuration
    baseUrl: 'http://localhost:3577',
    specPattern: [
      'cypress/features/**/*.feature',
      'cypress/tests/**/*.spec.ts*'
    ],
    excludeSpecPattern: [
      '**/snapshots/*',
      '**/examples/*',
      '**/*.skip.ts'
    ],
    
    // Test isolation and performance
    experimentalRunAllSpecs: true,
    experimentalModifyObstructiveThirdPartyCode: true,
    
    // Better error handling
    defaultCommandTimeout: 10000,
    requestTimeout: 10000,
    responseTimeout: 10000,
    
    // Screenshot and video settings
    screenshotsFolder: 'cypress/screenshots',
    videosFolder: 'cypress/videos',
  },
  
  // Component testing (if needed later)
  component: {
    devServer: {
      framework: 'angular',
      bundler: 'webpack',
    },
    specPattern: 'cypress/component/**/*.cy.ts',
  },
});
