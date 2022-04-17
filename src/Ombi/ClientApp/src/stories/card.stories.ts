// // also exported from '@storybook/angular' if you can deal with breaking changes in 6.1

// import { Meta, Story } from '@storybook/angular/types-6-0';

// import Card from '../app/components/card/card.component';

// // More on default export: https://storybook.js.org/docs/angular/writing-stories/introduction#default-export
// export default {
//   title: 'Ombi/Card',
//   component: Card,
//   // More on argTypes: https://storybook.js.org/docs/angular/api/argtypes
//   argTypes: {
//     backgroundColor: { control: 'color' },
//   },
// } as Meta;

// // More on component templates: https://storybook.js.org/docs/angular/writing-stories/introduction#using-args
// const Template: Story<Card> = (args: Card) => ({
//   props: args,
// });

// export const Primary = Template.bind({});
// // More on args: https://storybook.js.org/docs/angular/writing-stories/args
// Primary.args = {
//   primary: true,
//   label: 'Buttonaaaa',
// };

// export const Secondary = Template.bind({});
// Secondary.args = {
//   label: 'Buttonaaaa',
// };

// export const Large = Template.bind({});
// Large.args = {
//   size: 'large',
//   label: 'Buttonaaa',
// };

// export const Small = Template.bind({});
// Small.args = {
//   size: 'small',
//   label: 'Buttonaaa',
// };
