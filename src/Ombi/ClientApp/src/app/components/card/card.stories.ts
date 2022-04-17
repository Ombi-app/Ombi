// also exported from '@storybook/angular' if you can deal with breaking changes in 6.1

import { Meta, Story, moduleMetadata } from '@storybook/angular';

import { AppModule } from '../../app.module';
import Card from './card.component';
import { StorybookTranslateModule } from '../../../stories/storybook-translate-module';

// More on default export: https://storybook.js.org/docs/angular/writing-stories/introduction#default-export
export default {
  title: 'Ombi/Card',
  component: Card,
  // More on argTypes: https://storybook.js.org/docs/angular/api/argtypes
  argTypes: {
    backgroundColor: { control: 'color' },
  },
  decorators: [
    moduleMetadata({
      imports: [AppModule],
    }),
  ],
} as Meta;

// More on component templates: https://storybook.js.org/docs/angular/writing-stories/introduction#using-args
const Template: Story<Card> = (args: Card) => ({
  props: args,
});

export const Primary = Template.bind({});
// More on args: https://storybook.js.org/docs/angular/writing-stories/args
Primary.args = {
    result: {
        title: 'Title',
    }
};

