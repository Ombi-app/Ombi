// also exported from '@storybook/angular' if you can deal with breaking changes in 6.1
import { APP_BASE_HREF } from '@angular/common';
import { Story, Meta, moduleMetadata } from '@storybook/angular';
import { SocialIconsComponent } from './social-icons.component';
import { MatMenuModule } from "@angular/material/menu";
import { RequestType } from '../../../../interfaces';
import { userEvent, waitFor, within } from '@storybook/testing-library';
import { expect } from '@storybook/jest';

// More on default export: https://storybook.js.org/docs/angular/writing-stories/introduction#default-export
export default {
  title: 'Social Icons',
  component: SocialIconsComponent,
  decorators: [
    moduleMetadata({
      providers: [
        {
          provide: APP_BASE_HREF,
          useValue: ""
      },
      ],
      imports: [MatMenuModule]
    })
  ]
} as Meta;

// More on component templates: https://storybook.js.org/docs/angular/writing-stories/introduction#using-args
const Template: Story<SocialIconsComponent> = (args: SocialIconsComponent) => ({
  props: args,
});

export const All = Template.bind({});
// More on args: https://storybook.js.org/docs/angular/writing-stories/args
All.args = {
  twitter: "test",
  homepage: "test",
  theMoviedbId: 1,
  hasTrailer: true,
  imdbId: "test",
  tvdbId: "test",
  facebook: "test",
  instagram: "test",
  available: true,
  doNotAppend: false,
  type: RequestType.movie,
  isAdmin: false,
  canShowAdvanced: false,
  has4KRequest: false
};

export const Admin = Template.bind({});
Admin.args = {
  twitter: "test",
  homepage: "test",
  theMoviedbId: 1,
  hasTrailer: true,
  imdbId: "test",
  tvdbId: "test",
  facebook: "test",
  instagram: "test",
  available: true,
  doNotAppend: false,
  type: RequestType.movie,
  isAdmin: true,
  canShowAdvanced: true,
  has4KRequest: true
};