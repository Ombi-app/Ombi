module.exports = {
  "stories": [
    "../src/**/*.stories.mdx",
    "../src/**/*.stories.@(js|jsx|ts|tsx)"
  ],
  "addons": [
    "@storybook/addon-links",
    "@storybook/addon-essentials",
    "@storybook/addon-interactions",
    "@storybook/preset-scss",
  ],
  "framework": "@storybook/angular",
  "core": {
    "builder": "@storybook/builder-webpack5"
  },
    "staticDirs": [{ from: '../../wwwroot/images', to: 'images' }, { from: '../../wwwroot/translations', to: 'translations'}],
  "features": {
    interactionsDebugger: true,
  }
}