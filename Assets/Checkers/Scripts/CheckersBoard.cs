using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Checkers
{
    public class CheckersBoard : MonoBehaviour
    {
        [Tooltip("Prefabs for Checker Pieces")]
        public GameObject whitePiecePrefab, blackPiecePrefab;
        [Tooltip("Where to attach the spawned pieces in the Hierarchy")]
        public Transform checkersParent;
        public Vector3 boardOffset = Vector3.zero;
        public Vector3 pieceOffset = new Vector3(.5f, 0, .5f);
        public float rayDistance = 1000f;
        public LayerMask hitLayers;
        public Piece[,] pieces = new Piece[8, 8];

        private Piece selectedPiece = null;
        private Vector2 mouseOver, startDrag, endDrag;
        private bool isWhiteTurn = true, hasKilled;


        void Start()
        {
            GenerateBoard();
        }

        private void Update()
        {
            MouseOver();
            // is it currently white's turn
            if (isWhiteTurn)
            {
                // get x and y coordinated of selected mouse over
                int x = (int)mouseOver.x;
                int y = (int)mouseOver.y;
                // detect selected piece 
                
                // if the mouse is pressed 
                if (Input.GetMouseButtonDown(0))
                {
                    // try selecting piece 
                    selectedPiece = SelectPiece(x, y);
                    startDrag = new Vector2(x, y);
                }
                // if there is a selected piece 
                if (selectedPiece)
                {
                    // move the piece with the mouse
                    DragPiece(selectedPiece);
                }

                if(Input.GetMouseButtonUp(0))
                {
                    endDrag = new Vector2(x, y);
                    TryMove(startDrag, endDrag); // finish dragging the piece
                    selectedPiece = null; // let go of the current piece 
                }
            }
        }

        /// <summary>
        /// Generates a Checker Piece in specified coordinates
        /// </summary>
        /// <param name="x">X Location</param>
        /// <param name="y">Y Location</param>
        public void GeneratePiece(int x, int y, bool isWhite)
        {
            // What prefab are we using (white or black) ?
            GameObject prefab = isWhite ? whitePiecePrefab : blackPiecePrefab;
            // Generate Instance of prefab
            GameObject clone = Instantiate(prefab, checkersParent);
            // get the piece component
            Piece p = clone.GetComponent<Piece>();
            // Update Piece Location
            p.x = x;
            p.y = y;
            // Reposition clone
            MovePiece(p, x, y);
        }

        /// <summary>
        /// Clears and re-generates entire board 
        /// </summary>
        public void GenerateBoard()
        {
            // Generate White Team
            for (int y = 0; y < 3; y++)
            {
                bool oddRow = y % 2 == 0;
                // Loop through columns
                for (int x = 0; x < 8; x += 2)
                {
                    // Generate Piece
                    GeneratePiece(oddRow ? x : x + 1, y, true);
                }
            }
            // Generate Black Team
            for (int y = 5; y < 8; y++)
            {
                bool oddRow = y % 2 == 0;
                // Loop through columns
                for (int x = 0; x < 8; x += 2)
                {
                    // Generate Piece
                    GeneratePiece(oddRow ? x : x + 1, y, false);
                }
            }
        }

        private Piece SelectPiece(int x, int y)
        {
            Piece result = null; // default result of function to null
            if (OutOfBounds(x, y))
            {
                return result;
            }
            // get the piece at the X and Y location
            Piece piece = pieces[x, y];
            // check that it isn't null
            if (piece)
            {
                result = piece;
            }

            return result;

        }

        private void MovePiece(Piece p, int x, int y)
        {
            pieces[p.x, p.y] = null;
            pieces[x, y] = p;
            p.x = x;
            p.y = y;
            // translate the piece to another location
            p.transform.localPosition = new Vector3(x, 0, y) + boardOffset + pieceOffset;
        }

        // updating when the pieces have been dragged
        private void MouseOver()
        {
            // peform raycast
            Ray CamRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(CamRay, out hit, rayDistance, hitLayers))
            {
                mouseOver.x = (int)(hit.point.x - boardOffset.x);
                mouseOver.y = (int)(hit.point.z - boardOffset.z);
            }
            else
            {
                mouseOver.x = -1;
                mouseOver.y = -1;
            }

        }

        /// <summary>
        /// Drags the selected piece using raycast location
        /// </summary>
        /// <param name="p"></param>
        private void DragPiece(Piece selected)
        {
            Ray CamRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            // detects the position of the selected piece 
            if (Physics.Raycast(CamRay, out hit, rayDistance, hitLayers))
            {
                selected.transform.position = hit.point + Vector3.up;
            }
        }

        private void TryMove(Vector2 start, Vector2 end)
        {
            int x1 = (int)start.x;
            int y1 = (int)start.y;
            int x2 = (int)end.x;
            int y2 = (int)end.y;
            // record start drag
            startDrag = new Vector2(x1, y1);
            //record end drag
            endDrag = new Vector2(x2, y2);
            
            if (selectedPiece)    
            {
                
                if (OutOfBounds(x2, y2))
                {
                    // move it back to the original (start) 
                    MovePiece(selectedPiece, x1, y1);
                }

                // check if it's a valid move       
                if (ValidMove(start, end))
                {
                    // replace the coordinated with out selected piece 
                    MovePiece(selectedPiece, x2, y2);
                }
                else
                {
                    //move it back to the original
                    MovePiece(selectedPiece, x1, y1);
                }   
            }
        }

        private bool OutOfBounds(int x, int y)
        {
            return x < 0 || x >= 8 || y < 0 || y >= 8;
        }

        private bool ValidMove(Vector2 start, Vector2 end)
        {
            int x1 = (int)start.x;
            int y1 = (int)start.y;
            int x2 = (int)end.x;
            int y2 = (int)end.y;

            if (start == end)
            {
                return true;
            }

            if (pieces[x2, y2]) // if you are moving on top of another piece
            {
                // ya cant do that boi
                return false;
            }
            // yeah you can do that 
            return true;
        }
    }
}