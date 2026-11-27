/*
 * This file is part of veraPDF WCAG Validation, a module of the veraPDF project.
 * Copyright (c) 2015-2026, veraPDF Consortium <info@verapdf.org>
 * All rights reserved.
 *
 * veraPDF WCAG Validation is free software: you can redistribute it and/or modify
 * it under the terms of either:
 *
 * The GNU General public license GPLv3+.
 * You should have received a copy of the GNU General Public License
 * along with veraPDF WCAG Validation as the LICENSE.GPL file in the root of the source
 * tree.  If not, see http://www.gnu.org/licenses/ or
 * https://www.gnu.org/licenses/gpl-3.0.en.html.
 *
 * The Mozilla Public License MPLv2+.
 * You should have received a copy of the Mozilla Public License along with
 * veraPDF WCAG Validation as the LICENSE.MPL file in the root of the source tree.
 * If a copy of the MPL was not distributed with this file, you can obtain one at
 * http://mozilla.org/MPL/2.0/.
 */
package org.verapdf.gf.model.impl.sa;

import org.verapdf.gf.model.factory.chunks.GraphicsState;
import org.verapdf.gf.model.factory.chunks.Matrix;
import org.verapdf.pd.PDResourcesHandler;
import org.verapdf.model.GenericModelObject;
import org.verapdf.model.baselayer.Object;
import org.verapdf.model.salayer.SAAnnotation;
import org.verapdf.model.salayer.SAChunk;
import org.verapdf.model.salayer.SAPage;
import org.verapdf.model.salayer.SATableBorder;
import org.verapdf.gf.model.impl.sa.tables.GFSATableBorder;
import org.verapdf.pd.PDAnnotation;
import org.verapdf.pd.PDPage;
import org.verapdf.wcag.algorithms.entities.IPage;
import org.verapdf.wcag.algorithms.entities.content.IChunk;
import org.verapdf.wcag.algorithms.entities.content.ImageChunk;
import org.verapdf.wcag.algorithms.entities.content.LineArtChunk;
import org.verapdf.wcag.algorithms.entities.content.TextChunk;
import org.verapdf.wcag.algorithms.entities.tables.tableBorders.TableBorder;
import org.verapdf.wcag.algorithms.semanticalgorithms.containers.StaticContainers;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

/**
 * Adapts one PDF page as an SA page, exposing chunks, annotations, and inferred table borders.
 *
 * <p>WCAG analysis consumes page-level visual artifacts alongside the tagged structure tree.
 * Content streams are parsed lazily using page resources and an initial graphics state; the page
 * rotation and crop box are used to map PDF coordinates to display coordinates. Annotation and
 * table-border links are assembled from the parser and the algorithms' shared analysis results.</p>
 *
 * <p><b>.NET porting notes:</b> Port as {@code VeraPdf.Validation.Wcag.Model.Impl.Sa.GFSAPage}
 * implementing {@code VeraPdf.Model.SALayer.ISAPage}. Use {@code VeraPdf.Parser.Pd.PdfPage} and
 * {@code IReadOnlyList<IModelObject>} for model links. Keep page index conventions consistent:
 * these values are zero-based, and the transformation's rotation branches must be ported exactly.</p>
 *
 * @author Maxim Plushchov
 */
public class GFSAPage extends GenericModelObject implements SAPage, IPage {

	/** Profile-visible model type name for pages in the semantic-analysis layer. */
	public static final String PAGE_TYPE = "SAPage";

	/** Model link name for non-semantic content chunks on this page. */
	private static final String ARTIFACTS = "artifacts";
	/** Model link name for table borders inferred on this page. */
	private static final String TABLE_BORDERS = "tableBorders";
	/** Model link name for page annotations. */
	private static final String ANNOTS = "annots";

	private GFSAContentStream contentStream = null;

	private List<SAChunk> artifacts = null;

	private final PDPage pdPage;

	private final String pageLabel;

	private List<SAAnnotation> annotations = null;

	/** Creates the page adapter with its PDF page and optional catalog page label. */
	public GFSAPage(PDPage pdPage, String pageLabel) {
		super(PAGE_TYPE);
		this.pdPage = pdPage;
		this.pageLabel = pageLabel;
	}

	@Override
	/** Dispatches profile link names to their lazy page-level collections. */
	public List<? extends Object> getLinkedObjects(String link) {
		switch (link) {
			case ARTIFACTS:
				return getartifacts();
			case ANNOTS:
				return this.getAnnotations();
			case TABLE_BORDERS:
				return this.getTableBorders();
			default:
				return super.getLinkedObjects(link);
		}
	}

	/** Wraps each parser annotation in its generic or link-specific SA adapter. */
	private List<SAAnnotation> parseAnnotations() {
		List<PDAnnotation> annots = pdPage.getAnnotations();
		if (!annots.isEmpty()) {
			List<SAAnnotation> res = new ArrayList<>(annots.size());
			for (PDAnnotation annot : annots) {
				res.add(GFSAAnnotation.createAnnot(annot, pdPage));
			}
			return Collections.unmodifiableList(res);
		}
		return Collections.emptyList();
	}

	/** Lazily initializes annotations and exposes a read-only list. */
	private List<SAAnnotation> getAnnotations() {
		if (this.annotations == null) {
			this.annotations = parseAnnotations();
		}
		return Collections.unmodifiableList(this.annotations);
	}

	/** Wraps the algorithm-detected table borders belonging to this page. */
	private List<SATableBorder> getTableBorders() {
		if (StaticContainers.getTableBordersCollection() == null) {
			return Collections.emptyList();
		}
		List<SATableBorder> tableBorders = new ArrayList<>();
		for (TableBorder tableBorder : StaticContainers.getTableBordersCollection().getTableBorders(pdPage.getPageNumber())) {
			tableBorders.add(new GFSATableBorder(tableBorder));
		}
		return tableBorders;
	}

	/** Adapts parsed artifact chunks to the matching SA model interface. */
	private List<SAChunk> getartifacts() {
		if (this.artifacts == null) {
			List<IChunk> artifacts = getArtifacts();
			this.artifacts = new ArrayList<>(artifacts.size());
			for (IChunk chunk : artifacts) {
				if (chunk instanceof TextChunk) {
					this.artifacts.add(new GFSATextChunk((TextChunk) chunk, ""));
				} else if (chunk instanceof ImageChunk) {
					this.artifacts.add(new GFSAImageChunk((ImageChunk) chunk));
				} else if (chunk instanceof LineArtChunk) {
					this.artifacts.add(new GFSALineArtChunk((LineArtChunk) chunk));
				}
			}
		}
		return artifacts;
	}

	@Override
	/** Returns parsed visual chunks, initializing the page content stream if needed. */
	public List<IChunk> getArtifacts() {
		if (contentStream == null) {
			parseContentStream();
		}
		if (contentStream != null) {
			return contentStream.getArtifacts();
		}
		return Collections.emptyList();
	}

	/** Returns the lazily constructed stream adapter used by document analysis. */
	public GFSAContentStream getContentStream() {
		if (this.contentStream == null) {
			parseContentStream();
		}
		return this.contentStream;
	}

	/** Builds the initial graphics state and binds page content to inherited resources. */
	private void parseContentStream() {
		GFSAContentStream pdContentStream = null;
		if (pdPage.getContent() != null) {
			PDResourcesHandler resourcesHandler = PDResourcesHandler.getInstance(pdPage.getResources(), pdPage.isInheritedResources());
			GraphicsState graphicsState = new GraphicsState(resourcesHandler);
			graphicsState.setCTM(createCurrentTransformationMatrix(pdPage));
			pdContentStream = new GFSAContentStream(pdPage.getContent(), graphicsState, resourcesHandler,
					pdPage.getPageNumber(), pdPage.getObject().getKey(), null, null, null);
		}
		this.contentStream = pdContentStream;
	}

	/** Builds the crop-box and rotation transform used for display-space geometry. */
	public static Matrix createCurrentTransformationMatrix(PDPage pdPage) {
		double[] cropBox = pdPage.getCropBox();
		if (cropBox == null) {
			cropBox = new double[]{0.0, 0.0, 0.0, 0.0};
		}
		Matrix currentTransformationMatrix = new Matrix();
		long rotation = pdPage.getRotation() % 360;
		if (rotation == 90L) {
			currentTransformationMatrix.translate(0, cropBox[2] - cropBox[0]);
			currentTransformationMatrix.rotate(1.5 * Math.PI);
		} if (rotation == 180L) {
			currentTransformationMatrix.translate(cropBox[2] - cropBox[0], cropBox[3] - cropBox[1]);
			currentTransformationMatrix.rotate(Math.PI);
		} if (rotation == 270L) {
			currentTransformationMatrix.translate(cropBox[3], -cropBox[0]);
			currentTransformationMatrix.rotate(0.5 * Math.PI);
		} else {
			currentTransformationMatrix.translate(-cropBox[0], -cropBox[1]);
		}
		return currentTransformationMatrix;
	}

	@Override
	public int getPageNumber() {
		return pdPage.getPageNumber();
	}

	@Override
	public String getPageLabel() {
		return pageLabel;
	}
}
