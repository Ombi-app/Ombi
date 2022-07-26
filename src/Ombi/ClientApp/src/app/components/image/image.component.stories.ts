// also exported from '@storybook/angular' if you can deal with breaking changes in 6.1
import { APP_BASE_HREF } from '@angular/common';
import { Story, Meta, moduleMetadata } from '@storybook/angular';
import { RequestType } from '../../interfaces';
import { ImageComponent } from './image.component';

// More on default export: https://storybook.js.org/docs/angular/writing-stories/introduction#default-export
export default {
  title: 'Image Component',
  component: ImageComponent,
  decorators: [
    moduleMetadata({
      providers: [
        {
          provide: APP_BASE_HREF,
          useValue: ""
      },
      ]
    })
  ]
} as Meta;

// More on component templates: https://storybook.js.org/docs/angular/writing-stories/introduction#using-args
const Template: Story<ImageComponent> = (args: ImageComponent) => ({
  props: args,
});

export const Primary = Template.bind({});
// More on args: https://storybook.js.org/docs/angular/writing-stories/args
Primary.args = {
  src: 'https://ombi.io/img/logo-orange-small.png',
  type: RequestType.movie
};

export const ClassApplied = Template.bind({});
ClassApplied.args = {
  src: 'https://ombi.io/img/logo-orange-small.png',
  type: RequestType.movie,
  class: 'test-class'
};

export const StyleApplied = Template.bind({});
StyleApplied.args = {
  src: 'https://ombi.io/img/logo-orange-small.png',
  type: RequestType.movie,
  style: 'background-color: red;'
};

export const IdApplied = Template.bind({});
IdApplied.args = {
  src: 'https://ombi.io/img/logo-orange-small.png',
  type: RequestType.movie,
  id: 'testId123'
};

// export const InvalidMovieImage = Template.bind({});
// InvalidMovieImage.args = {
//   src: 'https://httpstat.us/429',
//   type: RequestType.movie,
//   id: 'testId123'
// };

// export const InvalidTvImage = Template.bind({});
// InvalidTvImage.args = {
//   src: 'https://httpstat.us/429',
//   type: RequestType.tvShow,
// };

// export const InvalidMusicImage = Template.bind({});
// InvalidMusicImage.args = {
//   src: 'https://httpstat.us/429',
//   type: RequestType.album,
// };
