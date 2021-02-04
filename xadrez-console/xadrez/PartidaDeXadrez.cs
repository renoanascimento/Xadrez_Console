﻿using System.Collections.Generic;
using tabuleiro;

namespace xadrez
{
    class PartidaDeXadrez
    {
        public Tabuleiro tab { get; private set; }
        public int turno { get; private set; }
        public Cor jogadorAtual { get; private set; }
        public bool terminada { get; private set; }
        private HashSet<Peca> pecas;
        private HashSet<Peca> capturadas;
        public bool xeque { get; private set; }

        public PartidaDeXadrez()
        {
            tab = new Tabuleiro(8, 8);
            turno = 1;
            jogadorAtual = Cor.Branca;
            terminada = false;
            xeque = false;
            pecas = new HashSet<Peca>();
            capturadas = new HashSet<Peca>();
            ColocarPecas();
        }

        public Peca ExecutaMovimento(Posicao origem, Posicao destino)
        {
            Peca p = tab.RetirarPeca(origem);
            p.IncrementarQteMovimentos();
            Peca pecaCapturada = tab.RetirarPeca(destino);
            tab.ColocarPeca(p, destino);
            if (pecaCapturada != null)
            {
                capturadas.Add(pecaCapturada);
            }
            return pecaCapturada;
        }

        public void DesfazMovimento(Posicao origem, Posicao destino, Peca pecaCapturada)
        {
            Peca p = tab.RetirarPeca(destino);
            p.DecrementarQteMovimentos();
            if (pecaCapturada != null)
            {
                tab.ColocarPeca(pecaCapturada, destino);
                capturadas.Remove(pecaCapturada);
            }
            tab.ColocarPeca(p, origem);
        }

        public void RealizaJogada(Posicao origem, Posicao destino)
        {
            Peca pecaCapturada = ExecutaMovimento(origem, destino);
            if (EstaEmXeque(jogadorAtual))
            {
                DesfazMovimento(origem, destino, pecaCapturada);
                throw new TabuleiroException("Você não pode se colocar em xeque!");
            }
            if (EstaEmXeque(Adversaria(jogadorAtual)))
            {
                xeque = true;
            }
            else
            {
                xeque = false;
            }
            if (TesteXequeMate(Adversaria(jogadorAtual)))
            {
                terminada = true;
            }
            else
            {
                turno++;
                MudaJogador();
            }
        }

        public void ValidarPosicaoDeOrigem(Posicao pos)
        {
            if (tab.Peca(pos) == null)
            {
                throw new TabuleiroException("Não existe peça na posição escolhida!");
            }
            if (jogadorAtual != tab.Peca(pos).cor)
            {
                throw new TabuleiroException("A peça de origem escolhida não é sua!");
            }
            if (!tab.Peca(pos).ExisteMovimentosPossiveis())
            {
                throw new TabuleiroException("Não há movimentos possíveis para a peça de origem escolhida!");
            }
        }

        public void ValidarPosicaoDeDestino(Posicao origem, Posicao destino)
        {
            if (!tab.Peca(origem).MovimentoPossivel(destino))
            {
                throw new TabuleiroException("Posição de destino inválida!");
            }
        }

        private void MudaJogador()
        {
            if (jogadorAtual == Cor.Branca)
            {
                jogadorAtual = Cor.Preta;
            }
            else
            {
                jogadorAtual = Cor.Branca;
            }
        }

        public HashSet<Peca> PecasCapturadas(Cor cor)
        {
            HashSet<Peca> aux = new HashSet<Peca>();
            foreach (Peca x in capturadas)
            {
                if (x.cor == cor)
                {
                    aux.Add(x);
                }
            }
            return aux;
        }

        public HashSet<Peca> PecasEmJogo(Cor cor)
        {
            HashSet<Peca> aux = new HashSet<Peca>();
            foreach (Peca x in pecas)
            {
                if (x.cor == cor)
                {
                    aux.Add(x);
                }
            }
            aux.ExceptWith(PecasCapturadas(cor));
            return aux;
        }

        private Cor Adversaria(Cor cor)
        {
            if (cor == Cor.Branca)
            {
                return Cor.Preta;
            }
            else
            {
                return Cor.Branca;
            }
        }

        private Peca Rei(Cor cor)
        {
            foreach (Peca x in PecasEmJogo(cor))
            {
                if (x is Rei)
                {
                    return x;
                }
            }
            return null;
        }

        public bool EstaEmXeque(Cor cor)
        {
            Peca R = Rei(cor);
            if (R == null)
            {
                throw new TabuleiroException("Não tem Rei da cor " + cor + " no tabuleiro!");
            }
            foreach (Peca x in PecasEmJogo(Adversaria(cor)))
            {
                bool[,] mat = x.MovimentosPossiveis();
                if (mat[R.posicao.linha, R.posicao.coluna])
                {
                    return true;
                }
            }
            return false;
        }

        public bool TesteXequeMate (Cor cor)
        {
            if (!EstaEmXeque(cor))
            {
                return false;
            }
            foreach (Peca x in PecasEmJogo(cor))
            {
                bool[,] mat = x.MovimentosPossiveis();
                for (int i = 0; i < tab.linhas; i++)
                {
                    for (int j = 0; j < tab.colunas; j++)
                    {
                        if (mat[i, j])
                        {
                            Posicao origem = x.posicao;
                            Posicao destino = new Posicao(i, j);
                            Peca pecaCaputrada = ExecutaMovimento(origem, destino);
                            bool testaXeque = EstaEmXeque(cor);
                            DesfazMovimento(origem, destino, pecaCaputrada);
                            if (!testaXeque)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        public void ColocarNovaPeca(char coluna, int linha, Peca peca)
        {
            tab.ColocarPeca(peca, new PosicaoXadrez(coluna, linha).toPosicao());
            pecas.Add(peca);
        }

        private void ColocarPecas()
        {
            ColocarNovaPeca('A', 1, new Torre(tab, Cor.Branca));
            ColocarNovaPeca('B', 1, new Cavalo(tab, Cor.Branca));
            ColocarNovaPeca('C', 1, new Bispo(tab, Cor.Branca));
            ColocarNovaPeca('D', 1, new Dama(tab, Cor.Branca));
            ColocarNovaPeca('E', 1, new Rei(tab, Cor.Branca));
            ColocarNovaPeca('F', 1, new Bispo(tab, Cor.Branca));
            ColocarNovaPeca('G', 1, new Cavalo(tab, Cor.Branca));
            ColocarNovaPeca('H', 1, new Torre(tab, Cor.Branca));
            ColocarNovaPeca('A', 2, new Peao(tab, Cor.Branca));
            ColocarNovaPeca('B', 2, new Peao(tab, Cor.Branca));
            ColocarNovaPeca('C', 2, new Peao(tab, Cor.Branca));
            ColocarNovaPeca('D', 2, new Peao(tab, Cor.Branca));
            ColocarNovaPeca('E', 2, new Peao(tab, Cor.Branca));
            ColocarNovaPeca('F', 2, new Peao(tab, Cor.Branca));
            ColocarNovaPeca('G', 2, new Peao(tab, Cor.Branca));
            ColocarNovaPeca('H', 2, new Peao(tab, Cor.Branca));

            ColocarNovaPeca('A', 8, new Torre(tab, Cor.Preta));
            ColocarNovaPeca('B', 8, new Cavalo(tab, Cor.Preta));
            ColocarNovaPeca('C', 8, new Bispo(tab, Cor.Preta));
            ColocarNovaPeca('D', 8, new Dama(tab, Cor.Preta));
            ColocarNovaPeca('E', 8, new Rei(tab, Cor.Preta));
            ColocarNovaPeca('F', 8, new Bispo(tab, Cor.Preta));
            ColocarNovaPeca('G', 8, new Cavalo(tab, Cor.Preta));
            ColocarNovaPeca('H', 8, new Torre(tab, Cor.Preta));
            ColocarNovaPeca('A', 7, new Peao(tab, Cor.Preta));
            ColocarNovaPeca('B', 7, new Peao(tab, Cor.Preta));
            ColocarNovaPeca('C', 7, new Peao(tab, Cor.Preta));
            ColocarNovaPeca('D', 7, new Peao(tab, Cor.Preta));
            ColocarNovaPeca('E', 7, new Peao(tab, Cor.Preta));
            ColocarNovaPeca('F', 7, new Peao(tab, Cor.Preta));
            ColocarNovaPeca('G', 7, new Peao(tab, Cor.Preta));
            ColocarNovaPeca('H', 7, new Peao(tab, Cor.Preta));
        }
    }
}
