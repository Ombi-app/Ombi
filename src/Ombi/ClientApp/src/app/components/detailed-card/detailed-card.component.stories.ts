// also exported from '@storybook/angular' if you can deal with breaking changes in 6.1
import { APP_BASE_HREF } from '@angular/common';
import { Story, Meta, moduleMetadata } from '@storybook/angular';
import { IRecentlyRequested, RequestType } from '../../interfaces';
import { DetailedCardComponent } from './detailed-card.component';
import { TranslateModule } from "@ngx-translate/core";
import { ImageService } from '../../services/image.service';

// More on default export: https://storybook.js.org/docs/angular/writing-stories/introduction#default-export
export default {
  title: 'Detailed Card Component',
  component: DetailedCardComponent,
  decorators: [
    moduleMetadata({
      providers: [
        {
          provide: APP_BASE_HREF,
          useValue: {}
        },
      ],
      imports: [
        TranslateModule.forRoot(),
      ]
    })
  ]
} as Meta;

// More on component templates: https://storybook.js.org/docs/angular/writing-stories/introduction#using-args
const Template: Story<DetailedCardComponent> = (args: DetailedCardComponent) => ({
  props: args,
});

export const NewMovieRequest = Template.bind({});
// More on args: https://storybook.js.org/docs/angular/writing-stories/args
NewMovieRequest.args = {
  request: {
    title: 'The Matrix',
    approved: false,
    available: false,
    tvPartiallyAvailable: false,
    requestDate: new Date(2022, 1, 1),
    userName: 'John Doe',
    userId: '12345',
    requestType: RequestType.movie,
    mediaId: '603',
    overview: 'The Matrix is a movie about a group of people who are forced to fight against a powerful computer system that controls them.',
    releaseDate: new Date(2020, 1, 1),
    posterPath: "https://assets.fanart.tv/fanart/movies/603/movieposter/the-matrix-52256ae1021be.jpg"
  } as IRecentlyRequested,
  is4kEnabled: false,
};