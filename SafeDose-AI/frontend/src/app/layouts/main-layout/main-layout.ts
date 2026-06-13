import { Component } from '@angular/core';

import { RouterOutlet } from '@angular/router';
import { ChatBot } from '../../shared/components/chat-bot/chat-bot';
import { Header } from './components/header/header';
import { Footer } from './components/footer/footer';



@Component({
  selector: 'app-main-layout',
  imports: [Header, Footer, RouterOutlet, ChatBot],
  templateUrl: './main-layout.html',
  styleUrl: './main-layout.css',
  styles: [':host { display: block; }'],
})
export class MainLayout {}
