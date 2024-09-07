module.exports = {
	...require('@exclaimer/prettier-config'),
	overrides: [
		{
			files: ['*.yaml', '*.yml'],
			options: {
				tabWidth: 2,
				singleQuote: false,
			},
		},
		{
			files: 'index.html',
			options: {
				parser: 'html',
			},
		},
	],
};
